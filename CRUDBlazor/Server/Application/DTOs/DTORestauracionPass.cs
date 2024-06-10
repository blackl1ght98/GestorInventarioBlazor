using System.ComponentModel.DataAnnotations;

namespace CRUDBlazor.Server.Application.DTOs
{
    public class DTORestauracionPass
    {
        public string userId { get; set; }
        public string token { get; set; }

        public string password { get; set; }
        [Required]
        public string temporaryPassword { get; set; }
    }
}
