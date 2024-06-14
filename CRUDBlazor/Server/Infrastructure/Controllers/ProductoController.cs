using CRUDBlazor.Server.Helpers;
using CRUDBlazor.Server.Interfaces;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared;
using CRUDBlazor.Shared.Productos;
using MatBlazor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IGestorArchivos _gestorArchivos;
        private readonly ILogger<ProductoController> _logger;
        private readonly IProductoRepository _productoRepository;
        public ProductoController(DbcrudBlazorContext context, IGestorArchivos gestorArchivos, ILogger<ProductoController> logger, IProductoRepository producto)
        {
            _context = context;
            _gestorArchivos = gestorArchivos;
            _logger = logger;
            _productoRepository = producto;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductos([FromQuery] Paginacion paginacion, [FromQuery] int? idProveedor, [FromQuery] string buscar = "", [FromQuery] string ordenarPorPrecio = "")
        {
            try
            {
                var productos = _productoRepository.ObtenerTodosProductos();

               
                if (!string.IsNullOrEmpty(buscar))
                {
                    productos= productos.Where(s=>s.NombreProducto.Contains(buscar));
                }
                if (!string.IsNullOrEmpty(ordenarPorPrecio))
                {
                    productos = ordenarPorPrecio switch
                    {
                        "asc"=>productos.OrderBy(p=>p.Precio),
                        "desc"=>productos.OrderByDescending(p=>p.Precio),
                    };
                }
                if (idProveedor.HasValue)
                {
                    productos = productos.Where(p => p.IdProveedor == idProveedor.Value);
                }
                // var productos = _context.Productos.Include(x => x.IdProveedorNavigation).AsQueryable();
                await HttpContext.InsertarParametrosPaginacionRespuesta(productos, paginacion.CantidadAMostrar);
                var productoPaginado = productos.Paginar(paginacion).ToList();
                var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
                return Ok(productoPaginado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los productos paginados");
               return BadRequest("En este momento no se pueden obtener los productos intentelo mas tarde si el error persiste contacte con el admin");
            }
           
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductoPorId(int id)
        {
            try
            {
                var producto= await _productoRepository.ObtenerProductoPorId(id);
               // var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == id);
                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto por la id");
                return BadRequest("Error al obtener el producto por la id si el error persiste contacte con el administrador");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto(ProductosViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                    var (success,errorMessage)= await _productoRepository.CrearProducto(model);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest(errorMessage);
                    }
                }
                return Ok("Producto creado con exito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto");
                return BadRequest("En estos momentos no se ha podido crear el producto si el error persiste contacte con el administrador del sitio");
            }
           
          
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(string id)
        {
            try
            {
               
                var (success,errorMessage)=await _productoRepository.EliminarProducto(id);
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
                _logger.LogError(ex, "Error al eliminar el producto");
                return BadRequest("En estos momentos no se ha podido llevar a cabo la eliminacion del producto si el error persiste contacte con el administrador del sitio");
            }
            
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProducto(ProductosViewModel model)
        {
            try
            {           
                var (success,errorMessage)= await _productoRepository.EditarProducto(model);
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
                _logger.LogError(ex, "Error al actualizar el producto");
                return BadRequest("Error al actualizar el producto intentelo de nuevo mas tarde si el error persiste contacte con el administrador del sitio");
            }
            
        }

        [HttpPost("carrito")]
        public async Task<IActionResult> AgregarAlCarrito([FromBody] ProductoCarrito productoCarrito)
        {
            try
            {
               
                var (success,errorMessage)=await _productoRepository.AgregarAlCarrito(productoCarrito);
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
                _logger.LogError(ex, "Error al agregar producto al carrito");
                return BadRequest("Error al agregar el producto al carrito");
            }
            
        }
        



    }
}
