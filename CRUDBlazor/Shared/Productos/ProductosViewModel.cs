using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRUDBlazor.Shared.Productos
{
    public class ProductosViewModel
    {

        public int id { get; set; }
        [Required]
        public string nombreProducto { get; set; }
        [Required]
        public string descripcion { get; set; }
        [Required]
        public string? imagen { get; set; }
        public string? extension { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public double precio { get; set; }
        public string? TipoImagen { get; set; }

        public int idProveedor { get; set; }
        public ProveedorNavigation? idProveedorNavigation { get; set; }
    }
    public class ProveedorNavigation
    {
        public int? id { get; set; }
        public string? nombreProveedor { get; set; }
    }
}
