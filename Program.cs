using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FeatureFlagDemo;
using FeatureFlagDemo.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ Manually bind config section
var config = builder.Configuration;
var featureFlagConfig = new FeatureFlagConfig();
config.GetSection("FeatureFlagConfig").Bind(featureFlagConfig);

// ✅ Validate values
if (string.IsNullOrEmpty(featureFlagConfig.BaseUrl))
{
    throw new InvalidOperationException("FME_BASE_URL is missing in appsettings.json.");
}

if (string.IsNullOrEmpty(featureFlagConfig.EvaluatorApiKey))
{
    throw new InvalidOperationException("FME_EVALUATOR_API_KEY is missing in appsettings.json.");
}

// ✅ Register FeatureFlagConfig as singleton
builder.Services.AddSingleton(featureFlagConfig);

// ✅ Provide HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ✅ Register FeatureFlagService
builder.Services.AddScoped<FeatureFlagService>();

await builder.Build().RunAsync();

