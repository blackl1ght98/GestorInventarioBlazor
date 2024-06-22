using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Pedidos
{
    public class HistorialPedidosModel
    {
        public int id { get; set; }

        public DateTime? fecha { get; set; }

        public string? accion { get; set; }

        public string? ip { get; set; }

        public int? idUsuario { get; set; }
        public List<DetalleHistorialPedidosModel> detalleHistorialPedidos { get; set; }
    }
    public class DetalleHistorialPedidosModel
    {
        public int id { get; set; }

        public int? historialPedidoId { get; set; }

        public int? productoId { get; set; }

        public int? cantidad { get; set; }
        public string estadoPedido { get; set; }
        public string numeroPedido { get; set; }
        public string fechaPedido { get; set; }
        public ProductoModels producto { get; set; }
    }

    public class ProductoModels
    {
        public string nombreProducto { get; set; } = null!;

        public string descripcion { get; set; } = null!;

        public string? imagen { get; set; }

        public int cantidad { get; set; }

        public double precio { get; set; }

        public DateTime fechaCreacion { get; set; }

        public DateTime fechaModificacion { get; set; }

        public int? idProveedor { get; set; }
    }
    
}
