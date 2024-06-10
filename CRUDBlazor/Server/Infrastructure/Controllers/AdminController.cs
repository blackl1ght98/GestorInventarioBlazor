using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Application.Services;
using CRUDBlazor.Server.Helpers;
using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared;
using CRUDBlazor.Shared.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IEmailService _emailService;
        private readonly HashService _hashService;
        private readonly IConfirmEmailService _confirmEmailService;
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminRepository _adminRepository;

        public AdminController(DbcrudBlazorContext context, IEmailService emailService, HashService hashService, 
        IConfirmEmailService confirmEmailService, ILogger<AdminController> logger, IAdminRepository admin)
        {
            _context = context;
            _emailService = emailService;
            _hashService = hashService;
            _confirmEmailService = confirmEmailService;
            _logger = logger;
            _adminRepository = admin;
        }
       
        [HttpGet]
        public async Task<ActionResult> GetAllUsers([FromQuery] Paginacion paginacion)
        {
            try
            {
                 //var queryable = _context.Usuarios.Include(x => x.IdRolNavigation).AsQueryable();
                var queryable = _adminRepository.ObtenerUsuarios();
                await HttpContext.InsertarParametrosPaginacionRespuesta(queryable, paginacion.CantidadAMostrar);
                var usuarios= queryable.Paginar(paginacion).ToList();
                var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los usuarios");
               
                return BadRequest("En estos momentos no se ha podido llevar a cabo la obtención de los usuarios. Inténtelo de nuevo más tarde o contacte con el administrador.");
            }
        }
        [HttpGet("user/{id}")]
        public async Task<ActionResult> GetUserById(int id)
        {
            try
            {
                var user= await _adminRepository.ObtenerUsuarioPorId(id);
                //var user = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario por id");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la visualizacion de la vista de eliminacion de los datos del usuario intentelo de nuevo mas tarde o cantacte con el administrador");
            }
        }
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var roles= _adminRepository.ObtenerRoles();
                //var roles = _context.Roles.ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la obtención de los roles. Inténtelo de nuevo más tarde o contacte con el administrador.");
            }
        }
        [HttpPut("updateRole/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleViewModel model)
        {
            try
            {
                var (success,errorMessage)= await _adminRepository.ActualizarRol(id,model);
                if (success)
                {
                    return Ok(success);
                }
                else
                {
                    return BadRequest(errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la actualización del rol. Inténtelo de nuevo más tarde o contacte con el administrador.");
            }
        }
        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreacionUsuarioModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var (success,errorMessage)=await _adminRepository.CrearUsuario(model);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest(errorMessage);
                    }                
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                return BadRequest("En estos momentos no se ha podido llevar a cabo el registro del usuario. Inténtelo de nuevo más tarde o contacte con el administrador.");
            }
        }

        [AllowAnonymous]
        [HttpGet("confirmRegistration/{UserId}/{Token}")]
    
        public async Task<IActionResult> ConfirmRegistration(int UserId,
         string Token )
        {
            try
            {
                var usuarioDB = await _adminRepository.ConfirmarEmail(UserId);
                //var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == UserId);
                if (usuarioDB.ConfirmacionEmail != false)
                {
                    return BadRequest("Usuario ya validado con anterioridad");
                }

                if (usuarioDB.EnlaceCambioPass != Token)
                {
                    _logger.LogCritical("Intento de alteracion de token por el usuario " + usuarioDB.Id);
                    return BadRequest("Token no valido");
                }
                await _confirmEmailService.ConfirmEmail(new DTOConfirmRegistration
                {
                    UserId = UserId
                });
                return Redirect("https://localhost:7186/productos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar el email");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la confirmación de la cuenta. Inténtelo de nuevo más tarde o contacte con el administrador.");
            }
        }
        [Authorize]
        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Edit(UsuarioEditViewModel userVM)
        {
            var (success,errorMessage)= await _adminRepository.EditarUsuario(userVM);
            if (success)
            {
                return Ok(success);
            }
            else
            {
                return BadRequest(errorMessage);
            }
          
        } 
        [Authorize]
        [HttpDelete("deleteConfirmed/{id}")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var (success,errorMessage)= await _adminRepository.DeleteUsuario(id);
                if (success)
                {
                    return Ok(success);
                }
                else
                {
                    return BadRequest(errorMessage);
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la eliminacion de los datos del usuario intentelo de nuevo mas tarde o cantacte con el administrador");
            }
        }

    }

}
