using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FeatureFlagDemo;
using FeatureFlagDemo.Services;

using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read config value
var fmeBaseUrl = builder.Configuration["FME_BASE_URL"];
if (string.IsNullOrEmpty(fmeBaseUrl))
{
    throw new InvalidOperationException("FME_BASE_URL is missing in appsettings.json.");
}
builder.Services.AddSingleton(new FeatureFlagConfig { BaseUrl = fmeBaseUrl });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<FeatureFlagService>();

await builder.Build().RunAsync();
