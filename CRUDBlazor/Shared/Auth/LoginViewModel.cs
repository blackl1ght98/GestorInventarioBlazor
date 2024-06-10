using System.ComponentModel.DataAnnotations;

namespace CRUDBlazor.Shared.Auth
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string email { get; set; }
        [Required(ErrorMessage = "La contraseña es requerida.")]

        public string password { get; set; }
    }
}
