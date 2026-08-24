using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using POS.Client;
using POS.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthenticationStateProvider>());
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var authorization = sp.GetRequiredService<AuthorizationMessageHandler>();
    authorization.InnerHandler = new HttpClientHandler();
    return new HttpClient(authorization) { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5216/") };
});
await builder.Build().RunAsync();
