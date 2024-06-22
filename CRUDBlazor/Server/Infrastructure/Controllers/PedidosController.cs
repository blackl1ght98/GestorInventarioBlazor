using CRUDBlazor.Server.Helpers;
using CRUDBlazor.Server.Infrastructure.Repositories;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared;
using CRUDBlazor.Shared.Pedidos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly ILogger<PedidosController> _logger;
        private readonly IPedidoRepository _pedidoRepository;

        public PedidosController(DbcrudBlazorContext context, ILogger<PedidosController> logger, IPedidoRepository pedido)
        {
            _context = context;
            _logger = logger;
            _pedidoRepository = pedido;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPedidos([FromQuery] Paginacion paginacion, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string buscar = "")
        {
            try
            {
                var existeUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int usuarioId;
                if (int.TryParse(existeUsuario, out usuarioId))
                {

                    var pedidos = _pedidoRepository.ObtenerPedidos();

                    if (User.IsInRole("administrador"))
                    {


                        pedidos = _pedidoRepository.ObtenerPedidoAdmin();
                    }
                    else
                    {

                        pedidos = _pedidoRepository.ObtenerPedidoUsuario(usuarioId);
                    }
                    if (!string.IsNullOrEmpty(buscar))
                    {
                        pedidos = pedidos.Where(p => p.NumeroPedido.Contains(buscar) || p.EstadoPedido.Contains(buscar) || p.IdUsuarioNavigation.NombreCompleto.Contains(buscar));
                    }
                    if (fechaInicio.HasValue && fechaFin.HasValue)
                    {
                        pedidos = pedidos.Where(s => s.FechaPedido >= fechaInicio.Value && s.FechaPedido <= fechaFin.Value);
                    }
                    await HttpContext.InsertarParametrosPaginacionRespuesta(pedidos, paginacion.CantidadAMostrar);
                    var pedidosPaginados = await pedidos.Paginar(paginacion).ToListAsync();
                    var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
                    return Ok(pedidosPaginados);
                }
                else
                {
                    // Si no se puede obtener el usuario, devuelve un error de autorización
                    return Unauthorized("No tiene autorizacion para ver el contenido");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los pedidos");
                return BadRequest("Error al obtener los pedidos intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
        }
        [HttpGet("all/productos")]
        public async Task<IActionResult> GetAllProduct()
        {
            try
            {
                var productos = await _pedidoRepository.ObtenerProductosPedido();
                //var productos = await _context.Productos.ToListAsync();
                return Ok(productos);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos");
                return BadRequest("En este momento no se pueden obtener los productos intentelo mas tarde si el error persiste contacte con el admin");

            }

        }
        [HttpPost]
        public async Task<IActionResult> CrearPedido(PedidosViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var (success, errorMessage) = await _pedidoRepository.CrearPedido(model);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest(errorMessage);
                    }
                }
                return BadRequest("Hubo un error al crear un pedido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un pedido");
                return BadRequest("Error al crear un pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador ");
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetallePedido(string id)
        {
            try
            {
                int idInt = int.Parse(id);
                //var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos)
                //    .ThenInclude(p => p.Producto).Include(p => p.IdUsuarioNavigation)
                //    .FirstOrDefaultAsync(x => x.Id == idInt);
                var pedido = await _pedidoRepository.ObtenerDetallePedido(idInt);
                if (pedido == null)
                {
                    return BadRequest("Pedido no encontrado");
                }
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los detalles de cada pedido");
                return BadRequest("Error al obtener los detalles del pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }


        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(string id)
        {
            try
            {
                int idInt = int.Parse(id);

                var (success, errorMessage) = await _pedidoRepository.EliminarPedido(idInt);
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
                _logger.LogError(ex, "Error al eliminar el pedido");
                return BadRequest("Error al eliminar el pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPedido(EditarPedido model)
        {
            try
            {

                var (success, errorMessage) = await _pedidoRepository.EditarPedido(model);
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
                _logger.LogError(ex, "Error al editar el pedido");
                return BadRequest("Error al editar el pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }

        }
        [HttpGet("historialPedidos")]
        public async Task<IActionResult> GetHistorialPedidos([FromQuery] Paginacion paginacion, [FromQuery] string buscar = "")
        {
            var existeUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                var pedidos = from p in _context.HistorialPedidos.Include(dp => dp.DetalleHistorialPedidos).ThenInclude(p => p.Producto).Include(u => u.IdUsuarioNavigation)
                              select p;
                if (User.IsInRole("administrador"))
                {
                    pedidos = from p in _context.HistorialPedidos.Include(dp => dp.DetalleHistorialPedidos).ThenInclude(p => p.Producto).Include(u => u.IdUsuarioNavigation)
                              select p;
                }
                else
                {
                    pedidos = _context.HistorialPedidos.Where(p => p.IdUsuario == usuarioId)
                        .Include(dp => dp.DetalleHistorialPedidos).ThenInclude(p => p.Producto)
                        .Include(u => u.IdUsuarioNavigation);
                }
                if (!string.IsNullOrEmpty(buscar))
                {
                    pedidos = pedidos.Where(p => p.Accion.Contains(buscar));
                }
                await HttpContext.InsertarParametrosPaginacionRespuesta(pedidos, paginacion.CantidadAMostrar);
                var pedidosPaginados = await pedidos.Paginar(paginacion).ToListAsync();
                var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
                return Ok(pedidosPaginados);
            }
            return Unauthorized("Usuario no logueado");
        }
        [HttpGet("detalleHistorialPedido/{id}")]
        public async Task<IActionResult> DetallesHistorialPedido(string id)
        {
            var ide = int.Parse(id);
            var pedidos = await _context.HistorialPedidos
                .Include(p => p.DetalleHistorialPedidos)
                 .ThenInclude(d => d.Producto)
                .Where(p => p.Id == ide)
                .ToListAsync();

            return Ok(pedidos);
        }

        [HttpGet("descargarhistorialPDF")]
        public async Task<IActionResult> DescargarHistorialPDF()
        {
            var (success, errorMessage, pdfData) = await _pedidoRepository.DescargarPDF();
            if (!success)
            {
                return BadRequest(errorMessage);
            }
            return File(pdfData, "application/pdf", "historial.pdf");
        }
        [HttpDelete("deleteAllHistorial")]
        public async Task<IActionResult> DeleteAllHistorial()
        {
            var (success,errorMessage)= await _pedidoRepository.EliminarHistorial();
            if (success)
            {
                return Ok(success);
            }
            else
            {
                return BadRequest(errorMessage);
            }
        }


    }

}
