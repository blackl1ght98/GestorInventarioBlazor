using Aspose.Pdf;
using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Helpers;
using CRUDBlazor.Server.Interfaces;
using CRUDBlazor.Server.Interfaces.Application;
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
        private readonly IEmailService _emailService;
        public ProductoController(DbcrudBlazorContext context, IGestorArchivos gestorArchivos, ILogger<ProductoController> logger, 
            IProductoRepository producto, IEmailService email)
        {
            _context = context;
            _gestorArchivos = gestorArchivos;
            _logger = logger;
            _productoRepository = producto;
            _emailService = email;
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
                if (idProveedor.HasValue && idProveedor!=0)
                {
                    productos = productos.Where(p => p.IdProveedor == idProveedor.Value);
                }
                // var productos = _context.Productos.Include(x => x.IdProveedorNavigation).AsQueryable();
                await VerificarStock();
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
        public async Task VerificarStock()
        {
            try
            {
                var emailUsuario = User.FindFirstValue(ClaimTypes.Email);

                if (emailUsuario != null)
                {


                    var productos = from p in _context.Productos.Include(x => x.IdProveedorNavigation)
                                    orderby p.Id
                                    select p;


                    foreach (var producto in productos)
                    {
                        if (producto.Cantidad < 10) // Define tu propio umbral
                        {
                            await _emailService.SendEmailAsyncLowStock(new DTOEmail
                            {
                                ToEmail = emailUsuario
                            }, producto);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
               
                _logger.LogError("Error al verificar el stock", ex);
                
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
        [HttpGet("historialProducto")]
        public async Task<IActionResult> HistorialProducto([FromQuery] Paginacion paginacion, [FromQuery]string buscar="" )
        {
            var historialProductos = from p in _context.HistorialProductos.Include(x => x.DetalleHistorialProductos)
                                     select p;
            if (!string.IsNullOrEmpty(buscar))
            {
                historialProductos= historialProductos.Where(p=>p.Accion.Contains(buscar)|| p.Ip.Contains(buscar));
            }
            await HttpContext.InsertarParametrosPaginacionRespuesta(historialProductos, paginacion.CantidadAMostrar);
            var productos= historialProductos.Paginar(paginacion).ToList();
            var totalPaginas = HttpContext.Response.Headers["totalPaginas"].ToString();
            return Ok(productos);
        }
        [HttpGet("detalleHistorialProducto/{id}")]
        public async Task<IActionResult> DetallesHistorialProducto(string id)
        {
             int ide= int.Parse(id);
            var historialProducto= await _context.HistorialProductos.Include(hp=>hp.DetalleHistorialProductos).FirstOrDefaultAsync(hp=>hp.Id== ide);
            if(historialProducto == null)
            {
                return BadRequest("No hay detalles del producto");
            }
            //Cuando no se especifica el tipo de dato que devuelve o recibe el servidor siempre esperara un array de objetos
            return Ok(historialProducto);
        }
        [HttpGet("descargarhistorialPDF")]
        public async Task<IActionResult> DescargarHistorialPDF()
        {
            var historialProductos = await _context.HistorialProductos
               .Include(hp => hp.DetalleHistorialProductos)

               .ToListAsync();
            if (historialProductos == null || historialProductos.Count == 0)
            {
                return BadRequest("No hay productos a mostrar");
            }
            // Crear un documento PDF con orientación horizontal
            Document document = new Document();
            //Margenes y tamaño del documento
            document.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
            document.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;
            document.PageInfo.Margin = new MarginInfo(1, 10, 10, 10); // Ajustar márgenes
            // Agregar una nueva página al documento con orientación horizontal
            Page page = document.Pages.Add();
            //Control de margenes
            page.PageInfo.Margin.Left = 35;
            page.PageInfo.Margin.Right = 0;
            //Controla la horientacion actualmente es horizontal
            page.SetPageSize(Aspose.Pdf.PageSize.PageLetter.Width, Aspose.Pdf.PageSize.PageLetter.Height);
            // Crear una tabla para mostrar las mediciones
            Aspose.Pdf.Table table = new Aspose.Pdf.Table();
            table.VerticalAlignment = VerticalAlignment.Center;
            table.Alignment = HorizontalAlignment.Left;
            table.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
            table.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
            table.ColumnWidths = "55 50 45 45 45 35 45 45 45 45 35 50"; // Ancho de cada columna

            page.Paragraphs.Add(table);

            // Agregar fila de encabezado a la tabla
            Aspose.Pdf.Row headerRow = table.Rows.Add();
            headerRow.Cells.Add("Id").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("UsuarioId").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Fecha").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Accion").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Ip").Alignment = HorizontalAlignment.Center;

            // Agregar contenido de mediciones a la tabla
            foreach (var historial in historialProductos)
            {

                Aspose.Pdf.Row dataRow = table.Rows.Add();
                Aspose.Pdf.Text.TextFragment textFragment1 = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment1);
                dataRow.Cells.Add($"{historial.Id}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.UsuarioId}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Fecha}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Accion}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Ip}").Alignment = HorizontalAlignment.Center;

                // Crear una segunda tabla para los detalles del producto
                Aspose.Pdf.Table detalleTable = new Aspose.Pdf.Table();
                detalleTable.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
                detalleTable.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
                detalleTable.ColumnWidths = "100 60 60"; // Ancho de cada columna

                // Agregar la segunda tabla a la página
                page.Paragraphs.Add(detalleTable);
                Aspose.Pdf.Text.TextFragment textFragment = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment);
                // Agregar fila de encabezado a la segunda tabla
                Aspose.Pdf.Row detalleHeaderRow = detalleTable.Rows.Add();
                detalleHeaderRow.Cells.Add("NombreProducto").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Descripcion").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("IdHistorial").Alignment = HorizontalAlignment.Center;

                detalleHeaderRow.Cells.Add("Cantidad").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Precio").Alignment = HorizontalAlignment.Center;

                // Iterar sobre los DetalleHistorialProductos de cada HistorialProducto
                foreach (var detalle in historial.DetalleHistorialProductos)
                {
                    Aspose.Pdf.Row detalleRow = detalleTable.Rows.Add();

                    detalleRow.Cells.Add($"{detalle.NombreProducto}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.Descripcion}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.HistorialProductoId}").Alignment = HorizontalAlignment.Center;

                    detalleRow.Cells.Add($"{detalle.Cantidad}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.Precio}").Alignment = HorizontalAlignment.Center;
                }
            }
            // Crear un flujo de memoria para guardar el documento PDF
            MemoryStream memoryStream = new MemoryStream();
            // Guardar el documento en el flujo de memoria
            document.Save(memoryStream);
            // Convertir el documento a un arreglo de bytes
            byte[] bytes = memoryStream.ToArray();
            // Liberar los recursos de la memoria
            memoryStream.Close();
            memoryStream.Dispose();
            return File(bytes, "application/pdf", "historial.pdf");
        }

    }
}
