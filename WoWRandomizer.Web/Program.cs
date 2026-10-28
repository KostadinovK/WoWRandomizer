using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WoWRandomizer.Web;
using WoWRandomizer.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The UI is a separate app from the API: the API base URL comes from wwwroot/appsettings.json (ApiBaseUrl).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured. Set it in wwwroot/appsettings.json.");

// A trailing slash is required so relative request paths are appended to the base address.
var apiBaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");

builder.Services.AddHttpClient<IComboApiClient, ComboApiClient>(client => client.BaseAddress = apiBaseAddress);

await builder.Build().RunAsync();
