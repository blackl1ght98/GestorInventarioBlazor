using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Productos
{
    public class DescargarPDF
    {
        public int id { get; set; }

        public int? usuarioId { get; set; }

        public DateTime? fecha { get; set; }

        public string? accion { get; set; }

        public string? ip { get; set; }
        public List<DetalleDescargarPDF> detalles { get; set; }
    }
    public class DetalleDescargarPDF
    {
        public int id { get; set; }

        public int? historialProductoId { get; set; }

        public int? cantidad { get; set; }

        public string? nombreProducto { get; set; }

        public string? descripcion { get; set; }

        public decimal? Precio { get; set; }

    }
}
