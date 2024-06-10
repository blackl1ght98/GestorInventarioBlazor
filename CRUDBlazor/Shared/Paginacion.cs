using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared
{
    public class Paginacion
    {
        public int Pagina { get; set; } = 1;
        //Cuantos datos se va a mostrar por pagina
        public int CantidadAMostrar { get; set; } = 2;
    }
}
