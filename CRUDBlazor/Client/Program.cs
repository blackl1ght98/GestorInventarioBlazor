using CRUDBlazor.Client;

using CRUDBlazor.Client.Services;
using CRUDBlazor.Client.Services.Producto;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CustomHttpClient>();

builder.Services.AddOptions();
//Se usa para agregar los servicios de autorizacion, la autorizacion es lo que determina que puede y no puede hacer un usuario.
builder.Services.AddAuthorizationCore();
// Se registra 'ApiProperties' como un servicio con alcance 'Scoped'. Esto significa que una única instancia de 'ApiProperties' se
// reutilizará durante toda la vida útil de una solicitud HTTP en la aplicación. Esto es útil cuando necesitamos que los datos persistan
// y estén disponibles durante toda la interacción del usuario con la aplicación en una sesión específica.
builder.Services.AddScoped<ApiProperties>();
// Registra CustomAuthenticationStateProvider como la implementación de AuthenticationStateProvider.
//¿Que es AuthenticationStateProvider?
// AuthenticationStateProvider es una clase abstracta en Blazor que proporciona información sobre el estado de autenticación
// del usuario actual.
//----------------------------------------------------------------------------------------------------------------
// Al registrar CustomAuthenticationStateProvider como la implementación de AuthenticationStateProvider, estamos personalizando el
// comportamiento de la autenticación en nuestra aplicación.
// Por ejemplo, podemos tener lógica personalizada para determinar si un usuario está autenticado, obtener el usuario actualmente
// autenticado, etc.
//-----------------------------------------------------------------------------------------------
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
await builder.Build().RunAsync();
