using Aspose.Pdf;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.MetodosExtension;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared.Pedidos;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class PedidoRepository:IPedidoRepository
    {
        private readonly DbcrudBlazorContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public PedidoRepository(DbcrudBlazorContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _contextAccessor = accessor;
        }
        public  IQueryable<Pedido> ObtenerPedidos()
        {
            var pedido = from p in _context.Pedidos
                         select p;
            return pedido;
        }
        public  IQueryable<Pedido> ObtenerPedidoAdmin()
        {
            var pedido= _context.Pedidos.Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
            return pedido;
        }
        public IQueryable<Pedido> ObtenerPedidoUsuario(int usuarioId)
        {
            var pedido= _context.Pedidos.Where(p => p.IdUsuario == usuarioId)
                            .Include(dp => dp.DetallePedidos)
                            .ThenInclude(p => p.Producto)
                            .Include(u => u.IdUsuarioNavigation);
            return pedido;
        }
        public async Task<(bool, string)> CrearPedido(PedidosViewModel model)
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
                return (false, "Estado de pedido inválido");
            }
            _context.Add(pedido);
            await _context.SaveChangesAsync();

            var historialPedido = new HistorialPedido()
            {
                IdUsuario = pedido.IdUsuario,
                Fecha = DateTime.Now,
                Accion = _contextAccessor.HttpContext.Request.Method.ToString(),
                Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.Add(historialPedido);
            await _context.SaveChangesAsync();

            for (var i = 0; i < model.idsProducto.Count; i++)
            {
                if (model.productosSeleccionados[i])
                {
                    var detallePedido = new DetallePedido()
                    {
                        PedidoId = pedido.Id,
                        ProductoId = model.idsProducto[i],
                        Cantidad = model.cantidades[i],

                    };
                    _context.Add(detallePedido);
                    await _context.SaveChangesAsync();

                    var detalleHistorialPedido = new DetalleHistorialPedido()
                    {
                        HistorialPedidoId = historialPedido.Id,
                        ProductoId = model.idsProducto[i],
                        Cantidad = model.cantidades[i],
                        EstadoPedido = model.estadoPedido,
                        FechaPedido = model.fechaPedido,
                        NumeroPedido = model.numeroPedido
                    };
                    _context.Add(detalleHistorialPedido);
                    await _context.SaveChangesAsync();
                }
            }

            return (true, null);
        }

        public async Task<Pedido> ObtenerDetallePedido(int idInt)
        {
            var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos)
                   .ThenInclude(p => p.Producto).Include(p => p.IdUsuarioNavigation)
                   .FirstOrDefaultAsync(x => x.Id == idInt);
            return pedido;
        }
        public async Task<(bool,string)> EliminarPedido(int idInt)
        {
            var pedido = await _context.Pedidos.Include(dp => dp.DetallePedidos).FirstOrDefaultAsync(x => x.Id == idInt);
            if (pedido == null)
            {
                return (false,"Pedido no encontrado");
            }
           
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return (true, null);
        }
        public async Task<(bool, string)> EditarPedido(EditarPedido model)
        {
            var existeUsuario = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int usuarioId;
            if (int.TryParse(existeUsuario, out usuarioId))
            {
                var pedido = await _context.Pedidos.Include(x => x.DetallePedidos).FirstOrDefaultAsync(x => x.Id == model.id);
                if (pedido == null)
                {
                    return (false, "Pedido no encontrado");
                }

                // Crear un registro en el historial de pedidos antes de la actualización
                var historialPedidoAntes = new HistorialPedido
                {
                    IdUsuario = usuarioId,
                    Fecha = DateTime.Now,
                    Accion = "Antes-PUT",
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.AddEntity(historialPedidoAntes);

                foreach (var detalleOriginal in pedido.DetallePedidos)
                {
                    var nuevoDetalle = new DetalleHistorialPedido
                    {
                        HistorialPedidoId = historialPedidoAntes.Id,
                        ProductoId = detalleOriginal.ProductoId,
                        Cantidad = detalleOriginal.Cantidad,
                        EstadoPedido = pedido.EstadoPedido,
                        FechaPedido = pedido.FechaPedido,
                        NumeroPedido = pedido.NumeroPedido
                    };
                    _context.AddEntity(nuevoDetalle);
                }

                pedido.FechaPedido = model.fechaPedido;

                // Convierte la cadena al valor de enumeración correspondiente
                if (Enum.TryParse<EstadoPedido>(model.estadoPedido, out var estado))
                {
                    pedido.EstadoPedido = Enum.GetName(typeof(EstadoPedido), estado);
                }
                else
                {
                    return (false, "Estado de pedido inválido");
                }

                _context.UpdateEntity(pedido);

                // Crear un registro en el historial de pedidos después de la actualización
                var historialPedidoDespues = new HistorialPedido
                {
                    IdUsuario = usuarioId,
                    Fecha = DateTime.Now,
                    Accion = "Despues-PUT",
                  
                    Ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.AddEntity(historialPedidoDespues);

                foreach (var detalleOriginal in pedido.DetallePedidos)
                {
                    var nuevoDetalle = new DetalleHistorialPedido
                    {
                        HistorialPedidoId = historialPedidoDespues.Id,
                        ProductoId = detalleOriginal.ProductoId,
                        Cantidad = detalleOriginal.Cantidad,
                        EstadoPedido = pedido.EstadoPedido,
                        FechaPedido = pedido.FechaPedido,
                        NumeroPedido = pedido.NumeroPedido
                    };
                    _context.AddEntity(nuevoDetalle);
                }
            }

            return (true, null);
        }

        public async Task<List<Producto>> ObtenerProductosPedido()
        {
            var productos = await _context.Productos.ToListAsync();
            return productos;
        }
        public async Task<(bool, string, byte[])> DescargarPDF()
        {
            var historialPedido = await _context.HistorialPedidos
            .Include(hp => hp.DetalleHistorialPedidos)
            .ThenInclude(dp => dp.Producto)
            .ToListAsync();
            if (historialPedido == null || historialPedido.Count == 0)
            {
                return (false, "Datos de pedidos no encontrados", null);
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
            table.ColumnWidths = "55 50 45 45 60 "; // Ancho de cada columna

            page.Paragraphs.Add(table);

            // Agregar fila de encabezado a la tabla
            Aspose.Pdf.Row headerRow = table.Rows.Add();
            headerRow.Cells.Add("Id").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Accion").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Ip").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Id Usuario").Alignment = HorizontalAlignment.Center;
            headerRow.Cells.Add("Fecha").Alignment = HorizontalAlignment.Center;

            // Agregar contenido de mediciones a la tabla
            foreach (var historial in historialPedido)
            {

                Aspose.Pdf.Row dataRow = table.Rows.Add();
                Aspose.Pdf.Text.TextFragment textFragment1 = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment1);
                dataRow.Cells.Add($"{historial.Id}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Accion}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Ip}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.IdUsuario}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{historial.Fecha}").Alignment = HorizontalAlignment.Center;

                // Crear una segunda tabla para los detalles del producto
                Aspose.Pdf.Table detalleTable = new Aspose.Pdf.Table();
                detalleTable.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
                detalleTable.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
                detalleTable.ColumnWidths = "60 60 60 60"; // Ancho de cada columna

                // Agregar la segunda tabla a la página
                page.Paragraphs.Add(detalleTable);
                Aspose.Pdf.Text.TextFragment textFragment = new Aspose.Pdf.Text.TextFragment("");
                page.Paragraphs.Add(textFragment);
                // Agregar fila de encabezado a la segunda tabla
                Aspose.Pdf.Row detalleHeaderRow = detalleTable.Rows.Add();
                detalleHeaderRow.Cells.Add("Id Historial Ped.").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Id Producto").Alignment = HorizontalAlignment.Center;
                detalleHeaderRow.Cells.Add("Nombre Producto").Alignment = HorizontalAlignment.Center;

                detalleHeaderRow.Cells.Add("Cantidad").Alignment = HorizontalAlignment.Center;

                // Iterar sobre los DetalleHistorialProductos de cada HistorialProducto
                foreach (var detalle in historial.DetalleHistorialPedidos)
                {
                    Aspose.Pdf.Row detalleRow = detalleTable.Rows.Add();

                    detalleRow.Cells.Add($"{detalle.HistorialPedidoId}").Alignment = HorizontalAlignment.Center;
                    detalleRow.Cells.Add($"{detalle.ProductoId}");
                    detalleRow.Cells.Add($"{detalle.Producto?.NombreProducto}");
                    detalleRow.Cells.Add($"{detalle.Cantidad}").Alignment = HorizontalAlignment.Center;
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
            // Devolver el archivo PDF para descargar
            return (true, null, bytes);
        }
        public async Task<(bool, string)> EliminarHistorial()
        {
            var historialPedidos = await _context.HistorialPedidos.Include(x => x.DetalleHistorialPedidos).ToListAsync();
            if (historialPedidos == null || historialPedidos.Count == 0)
            {
                return (false, "No hay datos en el historial para eliminar");
            }
            // Eliminar todos los registros
            foreach (var historialProducto in historialPedidos)
            {
                _context.DeleteRangeEntity(historialProducto.DetalleHistorialPedidos);
                _context.DeleteEntity(historialProducto);

            }
            var detallePedidos = await _context.DetallePedidos.ToListAsync();
            foreach (var detallePedido in detallePedidos)
            {
                _context.DeleteEntity(detallePedido);
            }
            return (true, null);
        }
    }
 }

