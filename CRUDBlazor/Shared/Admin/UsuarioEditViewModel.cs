using System.ComponentModel.DataAnnotations;

namespace CRUDBlazor.Shared.Admin
{
    public class UsuarioEditViewModel
    {
        public int id { get; set; }

        [Required]
        public string email { get; set; } = null!;

        [Required]
        public string nombreCompleto { get; set; } = null!;

        [Required]
        public DateTime? fechaNacimiento { get; set; }

        [Required]
        public string? telefono { get; set; }

        [Required]
        public string direccion { get; set; } = null!;
    }
}
