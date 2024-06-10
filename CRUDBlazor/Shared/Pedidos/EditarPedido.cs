using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Pedidos
{
    public class EditarPedido
    {
        public int id { get; set; }
        public DateTime fechaPedido { get; set; }
        public string estadoPedido { get; set; }
    }
}
