
using CRUDBlazor.Server.Interfaces;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Productos;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class ProductoRepository: IProductoRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IGestorArchivos _gestorArchivos;
        private readonly ILogger<ProductoRepository> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        public ProductoRepository(DbcrudBlazorContext context, IGestorArchivos gestorArchivos, ILogger<ProductoRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _gestorArchivos = gestorArchivos;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }
        public IQueryable<Producto> ObtenerTodosProductos()
        {
            var producto = from p in _context.Productos.Include(x => x.IdProveedorNavigation).AsQueryable()
                           select p;
            return producto;
        }
        public async Task<Producto> ObtenerProductoPorId(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == id);
            return producto;
        }
        public async Task<(bool, string)> CrearProducto(ProductosViewModel model)
        {
            var producto = new Producto()
            {
                NombreProducto = model.nombreProducto,
                Descripcion = model.descripcion,
                Imagen = model.imagen,
                Cantidad = model.cantidad,
                Precio = model.precio,
                IdProveedor = model.idProveedor,

            };
            if (!string.IsNullOrEmpty(producto.Imagen))
            {
                var contenido = Convert.FromBase64String(producto.Imagen);
                var extension = model.extension; // Aquí necesitarás determinar la extensión de alguna manera
                producto.Imagen = await _gestorArchivos.GuardarArchivo(contenido, extension, "imagenes");
            }

            _context.Add(producto);
            await _context.SaveChangesAsync();

            return (true,null);
        }
        public async Task<(bool, string)> EliminarProducto(string id)
        {
            int idInt = int.Parse(id);
            var producto = await _context.Productos.Include(dp => dp.DetallePedidos)
                .ThenInclude(p => p.Pedido).Include(p => p.IdProveedorNavigation)
                .FirstOrDefaultAsync(x => x.Id == idInt);
            if (producto == null)
            {
                return (false,"El producto que intenta eliminar no se encuentra");
            }
            if (producto.DetallePedidos.Any())
            {
                return (false,"El producto no se puede eliminar porque tiene pedidos asociados");
            }
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return (true,"Producto eliminado con exito");
        }
        public async Task<(bool, string)> EditarProducto(ProductosViewModel model)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == model.id);
            if (producto == null)
            {
                return (false,"Producto no encontrado");
            }
            producto.NombreProducto = model.nombreProducto;
            producto.Descripcion = model.descripcion;
            producto.Cantidad = model.cantidad;
            producto.Precio = model.precio;
            producto.IdProveedor = model.idProveedor;
            if (!string.IsNullOrEmpty(model.imagen) && IsBase64String(model.imagen))
            {
                var contenido = Convert.FromBase64String(model.imagen);
                var extension = model.extension; // Aquí necesitarás determinar la extensión de alguna manera
                await _gestorArchivos.BorrarArchivo(producto.Imagen, "imagenes");
                producto.Imagen = await _gestorArchivos.GuardarArchivo(contenido, extension, "imagenes");
            }
            // Si model.imagen es null o una URL, no cambies producto.Imagen
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
            return (true,"Producto actualizado con exito");
        }
        private bool IsBase64String(string s)
        {
            try
            {
                // Crea un espacio en memoria (buffer) del mismo tamaño que la longitud de la cadena 's'.
                // Cuando editas, la cadena 's' almacena esto https://localhost:7186/imagenes/fbd208b2-32a3-448e-8e6d-86830ac0aead.jpg
                // En este caso, el enlace tiene 72 caracteres. El buffer se crea con la misma longitud.
                Span<byte> buffer = new Span<byte>(new byte[s.Length]);
                // Intenta convertir la cadena 's' a un array de bytes y lo almacena en 'buffer'.
                // Si 's' es una cadena Base64 válida, la conversión será exitosa y el método devolverá 'true'.
                // Si 's' no es una cadena Base64 válida (por ejemplo, es una URL), la conversión fallará y el método devolverá 'false'.
                // Aquí intenta convertir el link https://localhost:7186/imagenes/fbd208b2-32a3-448e-8e6d-86830ac0aead.jpg a una cadena base64. 
                // Como se le pasa un link, no es una cadena base 64 válida y devuelve false.
                return Convert.TryFromBase64String(s, buffer, out _);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar la conversion");
                return false;
            }

        }
        public async Task<(bool, string)> AgregarAlCarrito(ProductoCarrito productoCarrito)
        {
            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                var usuarioActual = usuarioId;
                var carrito = _context.Carritos.FirstOrDefault(c => c.UsuarioId == usuarioActual);
                if (carrito == null)
                {
                    carrito = new Carrito()
                    {
                        UsuarioId = usuarioActual,
                        FechaCreacion = DateTime.Now
                    };
                    _context.Carritos.Add(carrito);
                    await _context.SaveChangesAsync();
                }
                var itemCarrito = _context.ItemsDelCarritos.FirstOrDefault(i => i.CarritoId == carrito.Id && i.ProductoId == productoCarrito.idProducto);
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == productoCarrito.idProducto);
                if (producto != null)
                {
                    if (producto.Cantidad < productoCarrito.cantidad)
                    {
                        return (false,"No hay suficientes productos en stock");
                    }
                    if (itemCarrito == null)
                    {
                        itemCarrito = new ItemsDelCarrito
                        {
                            ProductoId = productoCarrito.idProducto,
                            Cantidad = productoCarrito.cantidad,
                            CarritoId = carrito.Id,
                        };
                        _context.ItemsDelCarritos.Add(itemCarrito);
                    }
                    else
                    {
                        itemCarrito.Cantidad += productoCarrito.cantidad;
                        _context.ItemsDelCarritos.Update(itemCarrito);
                    }
                    producto.Cantidad -= productoCarrito.cantidad;
                    _context.Productos.Update(producto);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                return (true,"Producto creado con exito");
            }
            return (false,"El producto no puede ser nulo");
        }
    }
}
