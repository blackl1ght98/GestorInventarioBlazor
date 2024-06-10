using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Admin
{
    public class CreacionUsuarioModel
    {
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        public int idRol { get; set; } = 1;
        [Required]
        public string nombreCompleto { get; set; }
        [Required]
        public DateTime? fechaNacimiento { get; set; }
        public string telefono { get; set; }
        public string direccion { get; set; }
    }
}
