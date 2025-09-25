using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OnePortal.Blazor;
using OnePortal.Blazor.Services.Auth;
using OnePortal.Blazor.Services.Http;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// (optional) logging…
Log.Logger = OnePortal.Blazor.Services.Logging.SerilogConfig.CreateClientLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Load API base URL (however you currently load it)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7082/";

// ---- Core auth services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthApi>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<WebAuthnClient>();


// ---- HTTP: register the handler, then add it to a named client
builder.Services.AddScoped<AuthMessageHandler>();                 // <<< REQUIRED

builder.Services.AddHttpClient("OnePortalAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthMessageHandler>();                     // <<< attach the handler

// Make the named client the default HttpClient injected everywhere
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("OnePortalAPI"));

Serilog.Log.Information("WASM client booting… with API Base URL {ApiBaseUrl}", apiBaseUrl);

await builder.Build().RunAsync();
