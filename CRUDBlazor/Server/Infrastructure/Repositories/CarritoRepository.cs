using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class CarritoRepository: ICarritoRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        public CarritoRepository(DbcrudBlazorContext context, IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _unitOfWork = unitOfWork;
        }
        public async Task<Carrito> ObtenerCarridoUsuario(int usuarioId)
        {
            var carrito = await _context.Carritos.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            return carrito;
        }
        public async Task<IQueryable<ItemsDelCarrito>> ObtenerItemsDelCarrito(int id)
        {
            var itemsDelCarrito = _context.ItemsDelCarritos
                           .Include(i => i.Producto)

                            .Include(i => i.Producto.IdProveedorNavigation)

                           .Where(i => i.CarritoId == id);
            return itemsDelCarrito;
        }
        public async Task<(bool, string,string)> Pagar()
        {
            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                var carrito = await _context.Carritos.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
                if (carrito != null)
                {
                    var itemsDelCarrito = await _context.ItemsDelCarritos.Where(i => i.CarritoId == carrito.Id).ToListAsync();
                    if (itemsDelCarrito.Count == 0)
                    {
                        return (false,"No hay productos en el carrito",null);
                    }
                    var pedido = new Pedido()
                    {
                        NumeroPedido = GenerarNumeroPedido(),
                        FechaPedido = DateTime.Now,
                        EstadoPedido = "Pendiente",
                        IdUsuario = usuarioId
                    };
                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();
                    foreach (var item in itemsDelCarrito)
                    {
                        var detallePedido = new DetallePedido()
                        {
                            PedidoId = pedido.Id,
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad ?? 0,
                        };
                        _context.DetallePedidos.Add(detallePedido);
                    }
                    _context.ItemsDelCarritos.RemoveRange(itemsDelCarrito);
                    await _context.SaveChangesAsync();

                    var items = new List<Item>();
                    decimal totalAmount = 0;
                    foreach (var item in itemsDelCarrito)
                    {
                        var producto = await _context.Productos.FindAsync(item.ProductoId);
                        var paypalItem = new Item()
                        {
                            name = producto.NombreProducto,
                            currency = "EUR",
                            price = producto.Precio.ToString("0.00"),
                            quantity = item.Cantidad.ToString(),
                            sku = "producto"
                        };
                        items.Add(paypalItem);
                        await _context.SaveChangesAsync();
                        totalAmount += Convert.ToDecimal(producto.Precio) * Convert.ToDecimal(item.Cantidad ?? 0);
                    }
                    //string returnUrl = "https://localhost:7186/api/Payment";
                    string returnUrl = "https://localhost:7186/Payment/Success";
                    string cancelUrl = "https://localhost:7186/Payment/Cancel";
                    var createdPayment = await _unitOfWork.PaypalService.CreateOrderAsync(items, totalAmount, returnUrl, cancelUrl, "EUR");
                    //Cuando el pago es exitoso capturamos el valor que tenga approvalUrl y lo mandamos al controlador
                    var approvalUrlFromPayment = createdPayment.links.FirstOrDefault(x => x.rel.ToLower() == "approval_url")?.href;
                    return (true, "Los productos ha pasado a ser un pedido", approvalUrlFromPayment);
                }
                else
                {
                    return (false,"No hay productos en el carrito",null);
                }

            }
            return (false, null, null);
        }
        private string GenerarNumeroPedido()
        {
            var length = 10;
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
           .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
