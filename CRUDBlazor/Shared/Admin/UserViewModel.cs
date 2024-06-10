using System.ComponentModel.DataAnnotations;

namespace CRUDBlazor.Shared.Admin
{
    public class UserViewModel
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
        public int newRol { get; set; }

        //Porque se tiene que poner idRolNavigation y no se puede poner otro nombre el motivo es porque tiene que coincidir
        //exactamento con lo que devuelve la api.

        public RolViewModel idRolNavigation { get; set; }
    }
    public class RolViewModel
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }
}
