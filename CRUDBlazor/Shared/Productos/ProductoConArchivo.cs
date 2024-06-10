using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Productos
{
    public class ProductoConArchivo
    {
        public ProductosViewModel Model { get; set; }
        public IBrowserFile File { get; set; }
    }
}
