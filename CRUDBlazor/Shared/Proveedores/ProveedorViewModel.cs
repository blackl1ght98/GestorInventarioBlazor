using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Proveedores
{
    public class ProveedorViewModel
    {
        public int id { get; set; }
        public string nombreProveedor { get; set; }
        public string contacto { get; set; }
        public string direccion { get; set; }
    }
}
