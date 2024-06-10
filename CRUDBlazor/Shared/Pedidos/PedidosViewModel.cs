using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Pedidos
{
    public class PedidosViewModel
    {
        public int id { get; set; }
        [Display(Name = "Productos")]
        public List<int> idsProducto { get; set; } // Lista de IDs de productos, esta variable almacena el total de productos que hay
        public List<int> cantidades { get; set; } // Lista de cantidades, esto almacena la cantidad de cada producto
        public List<bool> productosSeleccionados { get; set; }//estado del checkbook, al crear el pedido detecta cuales pedidos se han seleccionado
        public string numeroPedido { get; set; }
        public DateTime fechaPedido { get; set; }
        public string estadoPedido { get; set; }
        [Display(Name = "Clientes")]
        public int? idUsuario { get; set; }
        public List<DetallePedidoViewModel>? DetallePedidos { get; set; }
        public UsuarioViewModel? idUsuarioNavigation { get; set; }
    }
    public class DetallePedidoViewModel
    {
        public string nombreProducto { get; set; }
        public int cantidad { get; set; }
        public int? ProductoId { get; set; }
        public ProductoModel? producto { get; set; }
    }
    public class ProductoModel
    {
        public double? precio { get; set; }
        public string? nombreProducto { get; set; }
        public int cantidad { get; set; }
    }
    public class UsuarioViewModel
    {
        public int? id { get; set; }
        public string? nombreCompleto { get; set; }
    }

}
