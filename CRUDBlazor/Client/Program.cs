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
// Se registra 'ApiProperties' como un servicio con alcance 'Scoped'. Esto significa que una �nica instancia de 'ApiProperties' se
// reutilizar� durante toda la vida �til de una solicitud HTTP en la aplicaci�n. Esto es �til cuando necesitamos que los datos persistan
// y est�n disponibles durante toda la interacci�n del usuario con la aplicaci�n en una sesi�n espec�fica.
builder.Services.AddScoped<ApiProperties>();
// Registra CustomAuthenticationStateProvider como la implementaci�n de AuthenticationStateProvider.
//�Que es AuthenticationStateProvider?
// AuthenticationStateProvider es una clase abstracta en Blazor que proporciona informaci�n sobre el estado de autenticaci�n
// del usuario actual.
//----------------------------------------------------------------------------------------------------------------
// Al registrar CustomAuthenticationStateProvider como la implementaci�n de AuthenticationStateProvider, estamos personalizando el
// comportamiento de la autenticaci�n en nuestra aplicaci�n.
// Por ejemplo, podemos tener l�gica personalizada para determinar si un usuario est� autenticado, obtener el usuario actualmente
// autenticado, etc.
//-----------------------------------------------------------------------------------------------
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
await builder.Build().RunAsync();
