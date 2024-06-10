using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Carrito
{
    public class ObtencionItemsCarrito
    {
        public int carritoId { get; set; }

        public int? usuarioId { get; set; }

        public DateTime? fechaCreacion { get; set; }

        public List<ItemCarritoPersonalizado> itemsDelCarrito { get; set; } = new List<ItemCarritoPersonalizado>();
    }

    public class ItemCarritoPersonalizado
    {
        public int id { get; set; }

        public int? productoId { get; set; }

        public int? cantidad { get; set; }

        public DetallePedidoPersonalizado detallePedido { get; set; }
    }

    public class DetallePedidoPersonalizado
    {
        public int id { get; set; }

        public int? pedidoId { get; set; }

        public int? productoId { get; set; }

        public int? cantidad { get; set; }
    }
}
