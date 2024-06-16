using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Productos
{
    public class HistorialProductoModel
    {
        public int id { get; set; }
        public int? usuarioId { get; set; }
        public DateTime? fecha { get; set; }
        public string? accion { get; set; }
        public string? ip { get; set; }
    }
}
