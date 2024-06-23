using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Carrito
{
    public class ItemsCarrito
    {
        public int Id { get; set; }
        public int? carritoId { get; set; }
        public int? productoId { get; set; }
        public int? cantidad { get; set; }
        public Productos producto { get; set; }
    }

    public class Productos
    {
        public int id { get; set; }
        public string nombreProducto { get; set; }
        public int cantidad { get; set; }
        public double precio { get; set; }
        public string  imagen { get; set; }

        public Proveedor idProveedorNavigation { get; set; }
    }

    public class Proveedor
    {
        public string nombreProveedor { get; set; }
    }
}
