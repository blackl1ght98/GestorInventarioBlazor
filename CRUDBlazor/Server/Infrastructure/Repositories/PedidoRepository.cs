using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class PedidoRepository:IPedidoRepository
    {
        private readonly DbcrudBlazorContext _context;

        public PedidoRepository(DbcrudBlazorContext context)
        {
            _context = context;
        }
        public  IQueryable<Pedido> ObtenerPedidos()
        {
            var pedido = from p in _context.Pedidos
                         select p;
            return pedido;
        }
        public  IQueryable<Pedido> ObtenerPedidoAdmin()
        {
            var pedido= _context.Pedidos.Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
            return pedido;
        }
        public IQueryable<Pedido> ObtenerPedidoUsuario(int usuarioId)
        {
            var pedido= _context.Pedidos.Where(p => p.IdUsuario == usuarioId)
                            .Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
            return pedido;
        }
        public async Task<(bool, string)> CrearPedido(PedidosViewModel model)
        {
            var pedido = new Pedido()
            {
                NumeroPedido = model.numeroPedido,
                FechaPedido = model.fechaPedido,

                IdUsuario = model.idUsuario,
            };
            if (Enum.TryParse<EstadoPedido>(model.estadoPedido, out var estado))
            {
                pedido.EstadoPedido = Enum.GetName(typeof(EstadoPedido), estado);
            }
            else
            {
                return (false,"Estado de pedido inválido");
            }
            _context.Add(pedido);
            await _context.SaveChangesAsync();

            for (var i = 0; i < model.idsProducto.Count; i++)
            {
                // Comprueba si el producto ha sido seleccionado
                if (model.productosSeleccionados[i])
                {
                    var detallePedido = new DetallePedido()
                    {
                        PedidoId = pedido.Id,
                        ProductoId = model.idsProducto[i],
                        Cantidad = model.cantidades[i],
                    };
                    _context.Add(detallePedido);
                }
            }

            await _context.SaveChangesAsync();
            return (true,null);
        }
        public async Task<Pedido> ObtenerDetallePedido(int idInt)
        {
            var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos)
                   .ThenInclude(p => p.Producto).Include(p => p.IdUsuarioNavigation)
                   .FirstOrDefaultAsync(x => x.Id == idInt);
            return pedido;
        }
        public async Task<(bool,string)> EliminarPedido(int idInt)
        {
            var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos).FirstOrDefaultAsync(x => x.Id == idInt);
            if (pedido == null)
            {
                return (false,"Pedido no encontrado");
            }
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return (true, null);
        }
        public async Task<(bool, string)> EditarPedido(EditarPedido model)
        {
            var pedido = await _context.Pedidos.FirstOrDefaultAsync(x => x.Id == model.id);
            if (pedido == null)
            {
                return (false,"Pedido no encontrado");
            }

            pedido.FechaPedido = model.fechaPedido;

            // Convierte la cadena al valor de enumeración correspondiente
            if (Enum.TryParse<EstadoPedido>(model.estadoPedido, out var estado))
            {
                pedido.EstadoPedido = Enum.GetName(typeof(EstadoPedido), estado);
            }
            else
            {
                return (false,"Estado de pedido inválido");
            }

            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
            return (true,null);
        }
        public async Task<List<Producto>> ObtenerProductosPedido()
        {
            var productos = await _context.Productos.ToListAsync();
            return productos;
        }

    }
 }

