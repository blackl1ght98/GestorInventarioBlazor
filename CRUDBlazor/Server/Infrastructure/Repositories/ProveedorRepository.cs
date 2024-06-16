using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Proveedores;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class ProveedorRepository:IProveedorRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public ProveedorRepository(DbcrudBlazorContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public IQueryable<Proveedore> ObtenerTodosProveedores()
        {
            var provider = from p in _context.Proveedores
                           select p;
            return provider;
        }
        public async Task<(bool, string)> CrearProveedor(ProveedorViewModel model)
        {
            var proveedor = new Proveedore()
            {
                NombreProveedor = model.nombreProveedor,
                Contacto = model.contacto,
                Direccion = model.direccion,
            };
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return (true,"https://localhost:7186/");
        }
        public async Task<Proveedore> ObtenerProveedorId(int id)
        {
            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == id);
            return proveedor;
        }
        public async Task<(bool, string)> EliminarProveedor(string id)
        {
            int idInt = int.Parse(id);

            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == idInt);
            if (proveedor == null)
            {
                return (false,"Proveedor no encontrado");
            }
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            return (true,"Proveedor eliminado con exito");
        }
        public async Task<(bool, string)> EditarProveedor(ProveedorViewModel proveedor)
        {
            var existeProveedor = await _context.Proveedores.FindAsync(proveedor.id);
            if (existeProveedor == null)
            {
                return (false,"El proveedor que intenta editar no existe");
            }
            existeProveedor.NombreProveedor = proveedor.nombreProveedor;
            existeProveedor.Contacto = proveedor.contacto;
            existeProveedor.Direccion = proveedor.direccion;
            _context.Entry(existeProveedor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return (true,"Proveedor editado con exito");
        }
    }
}
