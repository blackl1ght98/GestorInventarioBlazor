using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Productos
{
    public class DetalleHistorialProductoModel
    {
        public int id { get; set; }

        public int? historialProductoId { get; set; }

        public int? cantidad { get; set; }

        public string? nombreProducto { get; set; }

        public string? descripcion { get; set; }

        public decimal? precio { get; set; }
       

    }
    public class HistorialProductoResponse
    {
        public int id { get; set; }
        public int usuarioId { get; set; }
        public string fecha { get; set; }
        public string accion { get; set; }
        public string ip { get; set; }
        public DetalleHistorialProductoModel[] detalleHistorialProductos { get; set; }
    }
}
