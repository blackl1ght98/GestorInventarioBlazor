using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Auth;
using System.Security.Policy;

namespace CRUDBlazor.Server.Interfaces.Infrastructure
{
    public interface IAuthRepository
    {
        Task<Usuario> Loguear(LoginViewModel model);
        Task<Usuario> ObtenerEmail(string email);
        Task<(bool, string)> RestorePass(int userId, string token);
        Task<(bool, string)> RestorePassUser(DTORestauracionPass cambio);
        Task<Usuario> UsuarioPorId(int id);
      
    }
}
