using CRUDBlazor.Server.Models;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface ICarritoRepository
    {
        Task<Carrito> ObtenerCarridoUsuario(int usuarioId);
        Task<IQueryable<ItemsDelCarrito>> ObtenerItemsDelCarrito(int id);
        Task<(bool, string, string)> Pagar();
    }
}
