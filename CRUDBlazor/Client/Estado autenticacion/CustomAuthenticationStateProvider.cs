using CRUDBlazor.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
/*AuthenticationStateProvider--> Esto le dice al cliente de blazor si el usuario esta autenticado
 o no*/
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    //Este servicio ayuda a manejar el estado de autenticacion
    private AuthenticationState _cachedAuthState;

    public CustomAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
       
    }

    // Este método asincrónico obtiene el estado de autenticación actual.
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Si el estado de autenticación ya está en caché, lo devuelve.
        if (_cachedAuthState != null)
        {
            return _cachedAuthState;
        }

        // Realiza una solicitud GET a la API para verificar si el usuario está autenticado.
        var response = await _httpClient.GetAsync("api/Auth/isAuthenticated");

        // Inicializa la variable authInfo como null.
        AuthState authInfo = null;

        // Si la respuesta de la API es exitosa, procesa la respuesta.
        if (response.IsSuccessStatusCode)
        {
            // Lee el contenido de la respuesta como una cadena.
            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserializa la cadena JSON en un objeto AuthInfo.
            authInfo = JsonSerializer.Deserialize<AuthState>(jsonString);
        }

        // Inicializa la variable identity.
        /*Los “claims” (reclamaciones) son una forma de representar los atributos del usuario en el 
         * mundo de la seguridad y la identidad. En términos de autenticación y autorización, un “claim” 
         * es una declaración que una entidad (generalmente un usuario) hace acerca de sí misma.

        Por ejemplo, un claim puede ser tu nombre, tu dirección de correo electrónico, tu edad, tu rol en 
        una organización, etc. Estos claims se utilizan para tomar decisiones de autorización en tu 
        aplicación. Por ejemplo, puedes tener una regla que dice que solo los usuarios con el claim 
        de “Admin” pueden acceder a ciertas áreas de tu aplicación.

        En el contexto de .NET y ASP.NET Core Identity, los claims se utilizan para construir la 
        identidad del usuario después de que se ha autenticado. Esta “identidad” es básicamente una 
        colección de claims. Por ejemplo, cuando un usuario se autentica, puedes crear un claim para 
        su ID de usuario, otro para su nombre de usuario, otro para su dirección de correo electrónico, 
        etc. Luego, puedes usar estos claims para tomar decisiones de autorización en tu aplicación.
         */
        /*

    Autenticación: Es el proceso de verificar la identidad de un usuario. Esto suele hacerse 
        mediante un nombre de usuario y una contraseña, aunque también pueden utilizarse otros métodos, 
        como la autenticación de dos factores. Cuando te “logueas” en una aplicación, estás pasando por
        el proceso de autenticación.

    Autorización: Una vez que un usuario está autenticado, la autorización es el proceso de verificar 
        qué permisos tiene ese usuario. En otras palabras, qué secciones de la aplicación puede ver,
        qué acciones puede realizar, etc. Esto se determina a menudo por los “roles” o “claims” del usuario.

         */
        ClaimsIdentity identity;

        // Si authInfo no es null y el usuario está autenticado, procesa la información de autenticación.
        if (authInfo != null && authInfo.isAuthenticated)
        {
            // Inicializa una lista de claims.
            var claims = new List<Claim>();

            // Si el nombre del usuario no está vacío, agrega un claim de nombre.
            if (!string.IsNullOrEmpty(authInfo.nombre))
            {
                claims.Add(new Claim(ClaimTypes.Name, authInfo.nombre));
            }

            // Si el email del usuario no está vacío, agrega un claim de email.
            if (!string.IsNullOrEmpty(authInfo.email))
            {
                claims.Add(new Claim(ClaimTypes.Email, authInfo.email));
            }

            // Si el rol del usuario no está vacío, agrega un claim de rol.
            if (!string.IsNullOrEmpty(authInfo.role))
            {
                claims.Add(new Claim(ClaimTypes.Role, authInfo.role));
            }

            // Si el ID del usuario no es 0, agrega un claim de NameIdentifier.
            if (authInfo.id != 0)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, authInfo.id.ToString()));
            }

            // Crea una nueva identidad con los claims.
            identity = new ClaimsIdentity(claims, "apiauth_type");

            // Imprime cada claim en la consola.
            foreach (var claim in identity.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
        }
        else
        {
            // Si authInfo es null o el usuario no está autenticado, crea una nueva identidad sin claims.
            identity = new ClaimsIdentity();
        }

        // Crea un nuevo estado de autenticación con la identidad y lo guarda en caché.
        _cachedAuthState = new AuthenticationState(new ClaimsPrincipal(identity));

        // Devuelve el estado de autenticación.
        return _cachedAuthState;
    }


    public async Task UpdateAuthenticationStateAsync()
    {
        // Primero, se establece _cachedAuthState en null. Esto es para asegurarse de que la próxima vez que se solicite el estado de
        // autenticación, se obtendrá una nueva copia del servidor en lugar de utilizar la versión en caché.
        _cachedAuthState = null;

        // Luego, se llama al método GetAuthenticationStateAsync para obtener el estado de autenticación actual. Como _cachedAuthState se
        // estableció en null, este método hará una solicitud al servidor para obtener el estado de autenticación más reciente.
        var authState = await GetAuthenticationStateAsync();

        // Finalmente, se llama al método NotifyAuthenticationStateChanged para notificar a cualquier componente interesado que el estado
        // de autenticación ha cambiado. Se pasa el estado de autenticación como una tarea ya completada, ya que ya se ha obtenido el estado
        // de autenticación en el paso anterior.
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

}
