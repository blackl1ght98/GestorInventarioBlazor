using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Pedidos;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface IPedidoRepository
    {
        IQueryable<Pedido> ObtenerPedidos();
        IQueryable<Pedido> ObtenerPedidoAdmin();
        IQueryable<Pedido> ObtenerPedidoUsuario(int usuarioId);
        Task<(bool, string)> CrearPedido(PedidosViewModel model);
        Task<Pedido> ObtenerDetallePedido(int idInt);
        Task<(bool, string)> EliminarPedido(int idInt);
        Task<(bool, string)> EditarPedido(EditarPedido model);
        Task<List<Producto>> ObtenerProductosPedido();
    }
}
