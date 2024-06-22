
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Productos;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface IProductoRepository
    {
        IQueryable<Producto> ObtenerTodosProductos();
        Task<Producto> ObtenerProductoPorId(int id);
        Task<(bool, string)> CrearProducto(ProductosViewModel model);
        Task<(bool, string)> EliminarProducto(string id);
        Task<(bool, string)> EditarProducto(ProductosViewModel model);
        Task<(bool,string)> AgregarAlCarrito(ProductoCarrito productoCarrito);
        IQueryable<HistorialProducto> ObtenerHistorialProducto();
        Task<HistorialProducto> DetalleHistorialProductoId(string id);
        Task<(bool, string, byte[])> DescargarPDF();
        Task<(bool, string)> EliminarHistorial();
    }
}
