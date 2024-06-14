using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Proveedores;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface IProveedorRepository
    {
        IQueryable<Proveedore> ObtenerTodosProveedores();
        Task<(bool, string)> CrearProveedor(ProveedorViewModel model);
        Task<Proveedore> ObtenerProveedorId(int id);
        Task<(bool,string)> EliminarProveedor(string id);
        Task<(bool, string)> EditarProveedor(ProveedorViewModel proveedor);
    }
}
