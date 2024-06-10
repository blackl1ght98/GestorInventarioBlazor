using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Application.Services;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.MetodosExtension;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Auth;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class AuthRepository:IAuthRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly HashService _hashService;
        private readonly ILogger<AuthRepository> _logger;   
        public AuthRepository(DbcrudBlazorContext context, HashService  hash, ILogger<AuthRepository> logger)
        {
            _context = context;
            _hashService = hash;
            _logger = logger;
        }
        public async Task<Usuario> Loguear(LoginViewModel model)
        {
            var usuario = await _context.Usuarios.Include(x => x.IdRolNavigation).FirstOrDefaultAsync(u => u.Email == model.email);
            return usuario;
        }
        public async Task<Usuario> ObtenerEmail(string email)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
            return usuarioDB;
        }
        public async Task<(bool, string)> RestorePass(int userId, string token)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == userId);
            if (usuarioDB == null)
            {
                return (false,"Usuario no encontrado");
            }
            if (usuarioDB.EnlaceCambioPass != token)
            {
                return (false,"Token no valido");
            }
            if (usuarioDB.FechaEnlaceCambioPass < DateTime.Now)
            {
                usuarioDB.FechaEnlaceCambioPass = null;
                usuarioDB.TemporaryPassword = null;
                await _context.SaveChangesAsync();
                return (false,"La contraseña temporal ha expirado");

            }
            return (true,null);
        }
        public async Task<(bool, string)> RestorePassUser(DTORestauracionPass cambio)
        {
            var userId = int.Parse(cambio.userId);
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == userId);
            if (usuarioDB == null)
            {
                return (false,"Usuario no encontrado");
            }
            if (usuarioDB.EnlaceCambioPass != cambio.token)
            {
                return (false,"Token no valido");
            }
            var resultadoHashTemp = _hashService.Hash(cambio.temporaryPassword, usuarioDB.Salt);
            if (usuarioDB.TemporaryPassword != resultadoHashTemp.Hash)
            {
                return (false,"La contraseña temporal no es valida");
            }
            var resultadoHash = _hashService.Hash(cambio.password);
            usuarioDB.Password = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;

            _context.UpdateEntity(usuarioDB);
            return (true,null);
        }

        public async Task<Usuario> UsuarioPorId(int id)
        {
            var user = await _context.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }
    }
}
