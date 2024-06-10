using CRUDBlazor.Server.Interfaces.Application;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarritoController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly ILogger<CarritoController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICarritoRepository _carritoRepository;
        public CarritoController(DbcrudBlazorContext context, ILogger<CarritoController> logger, IUnitOfWork unitOfWork, ICarritoRepository carrito)
        {
            _context = context;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _carritoRepository = carrito;
        }
        [HttpGet]   
        public async Task<IActionResult> GetItemsCarrito()
        {
            try
            {
                var existeUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int usuarioId;
                if (int.TryParse(existeUsuario, out usuarioId))
                {
                    var carrito= await _carritoRepository.ObtenerCarridoUsuario(usuarioId);
                    //var carrito = await _context.Carritos.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
                    if (carrito != null)
                    {
                        
                        var itemsDelCarrito = await _carritoRepository.ObtenerItemsDelCarrito(carrito.Id);
                        return Ok(itemsDelCarrito);
                    }
                    return BadRequest("La id del usuario no se pudo convertir");
                }
                return Ok("Producto obtenido con exito");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los pedidos para el carrito");
                return BadRequest("Error al obtener los productos para el carrito");
            }   
        }
     
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            // Cambia la cultura actual del hilo a InvariantCulture
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            // Cambia la cultura de la interfaz de usuario actual del hilo a InvariantCulture
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
              
                var (success, errorMessage, aprovalURL) = await _carritoRepository.Pagar();
                if (aprovalURL != null)
                {
                    return Ok(aprovalURL);
                }
                else
                {
                    return BadRequest(errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar el checkout");
                return BadRequest("Error al realizar el checkout para tranformar los productos en el carrito");
            }

        }
        
    }
}
