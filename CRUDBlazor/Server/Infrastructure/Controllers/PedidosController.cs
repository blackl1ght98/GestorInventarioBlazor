using CRUDBlazor.Server.Helpers;
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

        public PedidosController(DbcrudBlazorContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }
        //[HttpGet]
        //public async Task<IActionResult> GetAllPedidos([FromQuery] Paginacion paginacion)
        //{
        //    var pedidos = _context.Pedidos.Include(dp => dp.DetallePedidos)
        //        .ThenInclude(p => p.Producto)
        //        .Include(u => u.IdUsuarioNavigation);
        //    await HttpContext.InsertarParametrosPaginacionRespuesta(pedidos, paginacion.CantidadAMostrar);
        //    var pedidosPaginados = await pedidos.Paginar(paginacion).ToListAsync();
        //    var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
        //    return Ok(pedidosPaginados);
        //}
        [HttpGet]
        public async Task<IActionResult> GetAllPedidos([FromQuery] Paginacion paginacion)
        {
            try
            {
                var existeUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int usuarioId;
                if (int.TryParse(existeUsuario, out usuarioId))
                {
                    //IQueryable permite construir consultas dinamicas con las que se pueden hacer bastantes cosas
                    IQueryable<Pedido> pedidos;

                    // Si el usuario es un administrador
                    if (User.IsInRole("administrador"))
                    {
                        // Muestra todos los pedidos
                        pedidos = _context.Pedidos.Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
                    }
                    else // Si el usuario no es un administrador
                    {
                        // Muestra solo los pedidos del usuario
                        pedidos = _context.Pedidos.Where(p => p.IdUsuario == usuarioId)
                            .Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
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

        [HttpPost]
        public async Task<IActionResult> CrearPedido(PedidosViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var pedido = new Pedido()
                    {
                        NumeroPedido = model.numeroPedido,
                        FechaPedido = model.fechaPedido,
                       
                        IdUsuario = model.idUsuario,
                    };
                    if (Enum.TryParse<EstadoPedido>(model.estadoPedido, out var estado))
                    {
                        pedido.EstadoPedido = Enum.GetName(typeof(EstadoPedido), estado);
                    }
                    else
                    {
                        return BadRequest("Estado de pedido inválido");
                    }
                    _context.Add(pedido);
                    await _context.SaveChangesAsync();

                    for (var i = 0; i < model.idsProducto.Count; i++)
                    {
                        // Comprueba si el producto ha sido seleccionado
                        if (model.productosSeleccionados[i])
                        {
                            var detallePedido = new DetallePedido()
                            {
                                PedidoId = pedido.Id,
                                ProductoId = model.idsProducto[i],
                                Cantidad = model.cantidades[i],
                            };
                            _context.Add(detallePedido);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Ok("Producto agregado con exito");
                }
                return BadRequest("Modelo no valido, si falta algun campo ponlo");
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
                var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos)
                    .ThenInclude(p => p.Producto).Include(p => p.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(x => x.Id == idInt);
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
                var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos).FirstOrDefaultAsync(x => x.Id == idInt);
                if (pedido == null)
                {
                    return NotFound("Pedido no encontrado");
                }
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                return Ok("Pedido eliminado con exito");

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
                var pedido = await _context.Pedidos.FirstOrDefaultAsync(x => x.Id == model.id);
                if (pedido == null)
                {
                    return NotFound("Pedido no encontrado");
                }

                pedido.FechaPedido = model.fechaPedido;

                // Convierte la cadena al valor de enumeración correspondiente
                if (Enum.TryParse<EstadoPedido>(model.estadoPedido, out var estado))
                {
                    pedido.EstadoPedido = Enum.GetName(typeof(EstadoPedido), estado);
                }
                else
                {
                    return BadRequest("Estado de pedido inválido");
                }

                _context.Pedidos.Update(pedido);
                await _context.SaveChangesAsync();
                return Ok("Pedido actualizado con éxito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el pedido");
                return BadRequest("Error al editar el pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
            
        }





    }
}
