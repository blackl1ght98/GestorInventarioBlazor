
using Aspose.Pdf;
using CRUDBlazor.Server.Interfaces;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.MetodosExtension;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Server.Models.ViewModels;
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
            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
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
                var historialProducto = new HistorialProducto()
                {
                    UsuarioId= usuarioId,
                    Fecha= DateTime.Now,
                    Accion= _contextAccessor.HttpContext.Request.Method.ToString(),
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.Add(historialProducto);
                await _context.SaveChangesAsync();
                var detalleHistorialProducto = new DetalleHistorialProducto()
                {
                    HistorialProductoId = historialProducto.Id,
                    Cantidad = producto.Cantidad,
                    NombreProducto = producto.NombreProducto,
                    Descripcion = producto.Descripcion,
                    Precio = (decimal)producto.Precio,
                };
                _context.Add(detalleHistorialProducto);
                await _context.SaveChangesAsync();

            }
               
            return (true,null);
        }
        public async Task<(bool, string)> EliminarProducto(string id)
        {

            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                int idInt = int.Parse(id);
                var producto = await _context.Productos.Include(dp => dp.DetallePedidos)
                    .ThenInclude(p => p.Pedido).Include(p => p.IdProveedorNavigation)
                    .FirstOrDefaultAsync(x => x.Id == idInt);
                if (producto == null)
                {
                    return (false, "El producto que intenta eliminar no se encuentra");
                }
                var productoOriginal = new ViewModelProducto()
                {
                    Id = producto.Id,
                    NombreProducto = producto.NombreProducto,
                    Descripcion = producto.Descripcion,
                    Cantidad = producto.Cantidad,
                    Precio = (decimal)producto.Precio,
                };
                var historialProducto = new HistorialProducto()
                {
                    UsuarioId = usuarioId,
                    Fecha = DateTime.Now,
                    Accion = "DELETE",
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.Add(historialProducto);
                await _context.SaveChangesAsync();
                var detalleHistorial = new DetalleHistorialProducto()
                {
                    HistorialProductoId = historialProducto.Id,
                    Cantidad = productoOriginal.Cantidad,
                    NombreProducto = productoOriginal.NombreProducto,
                    Descripcion = productoOriginal.Descripcion,
                    Precio = productoOriginal.Precio
                };
                _context.Add(detalleHistorial);
                await _context.SaveChangesAsync();
                if (producto.DetallePedidos.Any())
                {
                    return (false, "El producto no se puede eliminar porque tiene pedidos asociados");
                }
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
              
            return (true,"Producto eliminado con exito");
        }
        public async Task<(bool, string)> EditarProducto(ProductosViewModel model)
        {
            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == model.id);
                if (producto == null)
                {
                    return (false, "Producto no encontrado");
                }
                var productoOriginal = new ViewModelProducto()
                {
                    Id = producto.Id,
                    NombreProducto = producto.NombreProducto,
                    Descripcion = producto.Descripcion,
                    Cantidad = producto.Cantidad,
                    Precio = (decimal)producto.Precio,
                    Imagen = producto.Imagen,
                    IdProveedor = producto.IdProveedor
                };
                var historialProductoPostActualizacion = new HistorialProducto
                {
                    Fecha = DateTime.Now,
                    UsuarioId = usuarioId,
                    Accion = "Antes-PUT",
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.Add(historialProductoPostActualizacion);
                await _context.SaveChangesAsync();
                var detalleHistorialProductoPostActualizacion = new DetalleHistorialProducto
                {
                    HistorialProductoId = historialProductoPostActualizacion.Id,

                    Cantidad = productoOriginal.Cantidad,
                    NombreProducto = productoOriginal.NombreProducto,
                    Descripcion = productoOriginal.Descripcion,
                    Precio = productoOriginal.Precio,
                };
                _context.Add(detalleHistorialProductoPostActualizacion);
                await _context.SaveChangesAsync();
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
                var historialProducto = new HistorialProducto
                {
                    Fecha = DateTime.Now,
                    UsuarioId = usuarioId,
                    Accion = "Despues-PUT",
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.Add(historialProducto);
                await _context.SaveChangesAsync();
                var detalleHistorialProducto = new DetalleHistorialProducto
                {
                    HistorialProductoId = historialProducto.Id,

                    Cantidad = producto.Cantidad,
                    NombreProducto = producto.NombreProducto,
                    Descripcion = producto.Descripcion,
                    Precio = (decimal)producto.Precio,
                };
                _context.Add(detalleHistorialProducto);
                await _context.SaveChangesAsync();
                return (true, "Producto actualizado con exito");
            }
            return (false, "Usuario no autenticado");
            
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
        public IQueryable<HistorialProducto> ObtenerHistorialProducto()
        {
            var historialProductos = from p in _context.HistorialProductos.Include(x => x.DetalleHistorialProductos)
                                     select p;
            return historialProductos;
        }
        public async Task<HistorialProducto> DetalleHistorialProductoId(string id)
        {
            int ide = int.Parse(id);
            var historialProducto = await _context.HistorialProductos.Include(hp => hp.DetalleHistorialProductos).FirstOrDefaultAsync(hp => hp.Id == ide);
            return historialProducto;
        }
        public async Task<(bool, string, byte[])> DescargarPDF()
        {
            var historialProductos = await _context.HistorialProductos
               .Include(hp => hp.DetalleHistorialProductos)

               .ToListAsync();
            if (historialProductos == null || historialProductos.Count == 0)
            {
                return (false,"No hay productos a mostrar", null);
            }
            // Crear un documento PDF con orientación horizontal
            Document document = new Document();
            //Margenes y tamaño del documento
            document.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
            document.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;
            document.PageInfo.Margin = new MarginInfo(1, 10, 10, 10); // Ajustar márgenes
            // Agregar una nueva página al documento con orientación horizontal
            Page page = document.Pages.Add();
            //Control de margenes
            page.PageInfo.Margin.Left = 35;
            page.PageInfo.Margin.Right = 0;
            //Controla la horientacion actualmente es horizontal
            page.SetPageSize(Aspose.Pdf.PageSize.PageLetter.Width, Aspose.Pdf.PageSize.PageLetter.Height);
            // Crear una tabla para mostrar las mediciones
            Aspose.Pdf.Table table = new Aspose.Pdf.Table();
            table.VerticalAlignment = VerticalAlignment.Center;
            table.Alignment = HorizontalAlignment.Left;
            table.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
            table.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
            table.ColumnWidths = "55 50 45 49 45 35 45 45 45 45 35 50"; // Ancho de cada columna

            page.Paragraphs.Add(table);

            // Agregar fila de encabezado a la tabla
            Aspose.Pdf.Row headerRow = table.Rows.Add();
            headerRow.Cells.Add("Id").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("UsuarioId").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Fecha").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Accion").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Ip").Alignment = HorizontalAlignment.Center;

            // Agregar contenido de mediciones a la tabla
            foreach (var historial in historialProductos)
            {

                Aspose.Pdf.Row dataRow = table.Rows.Add();
                Aspose.Pdf.Text.TextFragment textFragment1 = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment1);
                dataRow.Cells.Add($"{historial.Id}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.UsuarioId}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Fecha}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Accion}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Ip}").Alignment = HorizontalAlignment.Center;

                // Crear una segunda tabla para los detalles del producto
                Aspose.Pdf.Table detalleTable = new Aspose.Pdf.Table();
                detalleTable.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
                detalleTable.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
                detalleTable.ColumnWidths = "100 60 60"; // Ancho de cada columna

                // Agregar la segunda tabla a la página
                page.Paragraphs.Add(detalleTable);
                Aspose.Pdf.Text.TextFragment textFragment = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment);
                // Agregar fila de encabezado a la segunda tabla
                Aspose.Pdf.Row detalleHeaderRow = detalleTable.Rows.Add();
                detalleHeaderRow.Cells.Add("NombreProducto").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Descripcion").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("IdHistorial").Alignment = HorizontalAlignment.Center;

                detalleHeaderRow.Cells.Add("Cantidad").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Precio").Alignment = HorizontalAlignment.Center;

                // Iterar sobre los DetalleHistorialProductos de cada HistorialProducto
                foreach (var detalle in historial.DetalleHistorialProductos)
                {
                    Aspose.Pdf.Row detalleRow = detalleTable.Rows.Add();

                    detalleRow.Cells.Add($"{detalle.NombreProducto}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.Descripcion}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.HistorialProductoId}").Alignment = HorizontalAlignment.Center;

                    detalleRow.Cells.Add($"{detalle.Cantidad}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.Precio}").Alignment = HorizontalAlignment.Center;
                }
            }
            // Crear un flujo de memoria para guardar el documento PDF
            MemoryStream memoryStream = new MemoryStream();
            // Guardar el documento en el flujo de memoria
            document.Save(memoryStream);
            // Convertir el documento a un arreglo de bytes
            byte[] bytes = memoryStream.ToArray();
            // Liberar los recursos de la memoria
            memoryStream.Close();
            memoryStream.Dispose();
            return (true, null, bytes);
        }
        public async Task<(bool, string)> EliminarHistorial()
        {
            var historialPedidos = await _context.HistorialProductos.Include(x => x.DetalleHistorialProductos).ToListAsync();
            if (historialPedidos == null || historialPedidos.Count == 0)
            {
                return (false, "No hay datos en el historial para eliminar");
            }
            // Eliminar todos los registros
            foreach (var historialProducto in historialPedidos)
            {
                _context.DeleteRangeEntity(historialProducto.DetalleHistorialProductos);
                _context.DeleteEntity(historialProducto);

            }
          
            return (true, null);
        }
    }
   
}
