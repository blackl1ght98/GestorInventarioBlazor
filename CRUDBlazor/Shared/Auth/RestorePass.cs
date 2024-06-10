using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Auth
{
    public class RestorePass
    {
        public string userId { get; set; }
        public string token { get; set; }
     
        public string password { get; set; }
        [Required]
        public string temporaryPassword { get; set; }
    }
}
