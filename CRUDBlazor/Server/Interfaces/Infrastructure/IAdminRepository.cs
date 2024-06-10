using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface IAdminRepository
    {
        IQueryable<Usuario> ObtenerUsuarios();
        Task<Usuario> ObtenerUsuarioPorId(int id);
        List<Role> ObtenerRoles();
        Task<(bool, string)> ActualizarRol(int id, [FromBody] UpdateRoleViewModel model);
        Task<(bool, string)> CrearUsuario(CreacionUsuarioModel model);
        Task<Usuario> ConfirmarEmail(int UserId);
        Task<(bool, string)> EditarUsuario(UsuarioEditViewModel userVM);
        Task<(bool, string)> DeleteUsuario(string id);
    }
}
