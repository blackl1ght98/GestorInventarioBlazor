using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Application.Services;
using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.MetodosExtension;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly HashService _hashService;
        private readonly IEmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthController(DbcrudBlazorContext context, HashService hashService, IEmailService emailService, TokenService tokenService, 
        ILogger<AuthController> logger, IAuthRepository auth)
        {
            _context = context;
            _hashService = hashService;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger;
            _authRepository = auth;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var user= await _authRepository.Loguear(model);
               // var user = await _context.Usuarios.Include(x => x.IdRolNavigation).FirstOrDefaultAsync(u => u.Email == model.email);
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }
                // Comprobar si el correo electrónico ha sido confirmado
                if (!user.ConfirmacionEmail)
                {
                    return Unauthorized("Por favor, confirma tu correo electrónico antes de iniciar sesión.");
                }
                //Se llama al servicio hash service
                var resultadoHash = _hashService.Hash(model.password, user.Salt);
                //Si la contraseña que se introduce es igual a la que hay en base de datos se procede al login
                if (user.Password != resultadoHash.Hash)
                {
                    _logger.LogWarning("Contraseña erronea, posible intento de hackeo");
                    return Unauthorized("El email y/o la contraseña son incorrectos.");
                }
                // Generar el token
                var tokenResponse = await _tokenService.GenerarToken(user);
                // Guardar el token en una cookie
                Response.Cookies.Append("auth", tokenResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromMinutes(60),
                    Secure = true,
                });
                return Ok(new { Token = tokenResponse.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar el login");
                return BadRequest("En estos momentos no se ha podido llevar a cabo el login del usuario intentelo de nuevo mas tarde o cantacte con el administrador");
            }
        }
        [AllowAnonymous]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("auth");
                return Redirect("https://localhost:7186/");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesion");
                return BadRequest("En estos momentos no se ha podido llevar a cabo el logout del usuario intentelo de nuevo mas tarde o cantacte con el administrador");

            }

        }
       //Reseteo de contraseña por etapas: Etapa 1
       /*En esta etapa lo primero que se hace es buscar al email en cuestio en base de datos, si dicho email existe, se le manda
        * un correo electronico este correo tendra una contraseña temporal que es la contraseña actual de la cuenta y un enlace de
        * restablecimiento.
        */
        [HttpGet("resetpassword/{email}")]
        public async Task<IActionResult> ResetPassword(string email)
        {
           // var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x=>x.Email==email);
           var usuarioDB= await _authRepository.ObtenerEmail(email);
            if (usuarioDB == null)
            {
                return NotFound("Email no encontrado");
            }
            await _emailService.SendEmailAsyncResetPassword(new DTOEmail
            {
                ToEmail = email,
            });
            return Ok("Email enviado con exito");
        }
        //Reseteo de contraseña por etapas: Etapa 2
        /*En esta etapa una vez que el usuario ha realizado clic en el enlace enviado al correo electronico se hacen varias comprobaciones
         * para ver si el usuario existe, si el token que contiene el enlace del correo no ha expirado o si los datos del mismo enlace no han 
         * expirado.
         */
        [HttpGet("restorepassword/{UserId}/{Token}")]
         public async Task<IActionResult> RestorePassword(int userId, string token)
        {
            
           
            var (success,errorMessage)= await _authRepository.RestorePass(userId, token);
            if (success)
            {
                return Ok(success);
            }
            else
            {
                return BadRequest(errorMessage);
            }

        }
        //Reseteo de contraseña por etapas: Etapa 3
        /*Esta es la etapa final que una vez comprobado lo anterior aqui lo primero se comprueban 2 datos criticos y si pasa la comprobacion
         * se procede a restablecer la contraseña
         */
        [HttpPost("restaurarpassword/{userId}/{token}")]
        public async Task<IActionResult> RestorePasswordUser(DTORestauracionPass cambio)
        {
           
            var (success,errorMessage)= await _authRepository.RestorePassUser(cambio);
            if (success)
            {
                return Ok(success);
            }
            else
            {
                return BadRequest(errorMessage);
            }
        }
        [HttpGet("isAuthenticated")]
        public async Task<IActionResult> IsAuthenticated()
        {
            try
            {
                var isAuthenticated = User.Identity.IsAuthenticated;
                string role = null;
                string nombre = null;
                string email = null;
                if (isAuthenticated)
                {
                   
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    //var user = await _context.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.Id == userId);
                    var user= await _authRepository.UsuarioPorId(userId);
                    role = user?.IdRolNavigation.Nombre;
                    nombre = user?.NombreCompleto;
                    email = user?.Email;
                }

                return Ok(new { isAuthenticated, role, nombre, email });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de autenticacion");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la autenticacion del usuario intentelo de nuevo mas tarde o cantacte con el administrador");

            }

        }



    }
}
