using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Auth
{
    public class AuthState
    {
        public int id { get; set; }

        public bool isAuthenticated { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }

    }
}
