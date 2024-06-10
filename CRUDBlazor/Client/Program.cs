using CRUDBlazor.Client;
using CRUDBlazor.Client.Services;
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
