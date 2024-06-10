using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Application.Services;
using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.MetodosExtension;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Operators;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class AdminRepository: IAdminRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly HashService _hashService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminRepository> _logger;
        public AdminRepository(DbcrudBlazorContext context, HashService hash, IEmailService email, ILogger<AdminRepository> logger)
        {
            _context = context;
            _hashService = hash;
            _emailService = email;
            _logger = logger;
        }
        public  IQueryable<Usuario> ObtenerUsuarios()
        {
            var usuarios = from u in _context.Usuarios.Include(x => x.IdRolNavigation)
                           select u;
            return usuarios;
        }
        public async Task<Usuario> ObtenerUsuarioPorId(int id)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
            return user;
        }
        public List<Role> ObtenerRoles()
        {
            var roles = _context.Roles.ToList();
            return roles;
        }
        public async Task<(bool, string)> ActualizarRol(int id, [FromBody] UpdateRoleViewModel model)
        {
            var user= await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return (false, "Usuario no encontrado");
            }
            user.IdRol = model.NewRole;
           await  _context.UpdateEntityAsync(user);
            return (true, null);
        }
        public async Task<(bool, string)> CrearUsuario(CreacionUsuarioModel model)
        {
            var existingUser = _context.Usuarios.FirstOrDefault(u => u.Email == model.email);
            if (existingUser != null)
            {
                return (false,"Este email ya está registrado.");
            }
            var resultadoHash = _hashService.Hash(model.password);
            var user = new Usuario()
            {
                Email = model.email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt,
                IdRol = model.idRol,
                NombreCompleto = model.nombreCompleto,
                FechaNacimiento = model.fechaNacimiento,
                Telefono = model.telefono,
                Direccion = model.direccion,
                FechaRegistro = DateTime.Now
            };
            await _context.AddEntityAsync(user);
            await _emailService.SendEmailAsyncRegister(new DTOEmail
            {
                ToEmail = model.email
            });
            return (true, null);
        }
        public async Task<Usuario> ConfirmarEmail(int UserId)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == UserId);
            return usuarioDB;
        }
        public async Task<(bool, string)> EditarUsuario(UsuarioEditViewModel userVM)
        {
            try
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == userVM.id);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }
                user.NombreCompleto = userVM.nombreCompleto;
                user.FechaNacimiento = userVM.fechaNacimiento;
                user.Telefono = userVM.telefono;
                user.Direccion = userVM.direccion;
                if (user.Email != userVM.email)
                {
                    user.ConfirmacionEmail = false;
                    user.Email = userVM.email;
                    _context.UpdateEntity(user);
                   
                    await _emailService.SendEmailAsyncRegister(new DTOEmail
                    {
                        ToEmail = userVM.email
                    });
                }
                else
                {
                    user.Email = userVM.email;
                }
                _context.EntityModified(user);
                return (true, null);
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar el usuario");
                var user = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == userVM.id);
                if (user == null)
                {
                    return (false, "Usuario no encontrado");
                }
                _context.Entry(user).Reload();

                user.NombreCompleto = userVM.nombreCompleto;
                user.FechaNacimiento = userVM.fechaNacimiento;
                user.Telefono = userVM.telefono;
                user.Direccion = userVM.direccion;
                if (user.Email != userVM.email)
                {
                    user.ConfirmacionEmail = false;
                    user.Email = userVM.email;
                    _context.Usuarios.Update(user);
                    await _context.SaveChangesAsync();
                    await _emailService.SendEmailAsyncRegister(new DTOEmail
                    {
                        ToEmail = userVM.email
                    });
                }
                else
                {
                    user.Email = userVM.email;
                }
                _context.EntityModified(user);
                return (true, null);
            }
            catch (Exception ex)
            {

                _logger.LogError("Ha ocurrido una excepcion al editar el usuario", ex);
                return (false, "Hubo un error al crear el usuario, intentelo de nuevo mas tarde");
            }
          
        }
        public async Task<(bool, string)> DeleteUsuario(string id)
        {
            int idInt = int.Parse(id);
            var user = await _context.Usuarios.Include(x => x.Pedidos).Include(x=>x.Carritos).FirstOrDefaultAsync(m => m.Id == idInt);
            if (user == null)
            {
                return (false,"Usuario no encontrado");
            }
            if (user.Pedidos.Any())
            {
                _logger.LogInformation("El usuario tiene pedidos asociados no se puede eliminar");
                return (false,"El usuario tiene pedidos asociados no se puede eliminar");
            }
            _context.DeleteRangeEntity(user.Carritos);
            _context.DeleteEntity(user);
            return (true, null);
        }
    }
}
