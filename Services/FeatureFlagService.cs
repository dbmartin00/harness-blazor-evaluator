using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeatureFlagDemo.Services
{
    public class FlagResult
    {
        public string Treatment { get; set; } = "control";
        public JsonElement? Config { get; set; } // Raw config JSON
    }

    public class FeatureFlagConfig
    {
        public string BaseUrl { get; set; } = "";
    }

    public class FeatureFlagService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flagUrlTemplate;

        public FeatureFlagService(HttpClient httpClient, FeatureFlagConfig config)
        {
            _httpClient = httpClient;

            var baseUrl = config.BaseUrl?.TrimEnd('/');
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("FME_BASE_URL is not configured.");
            }

            _flagUrlTemplate = $"{baseUrl}/client/get-treatments-with-config?key={{0}}&split-names={{1}}";
            Console.WriteLine($"FeatureFlagService initialized with base URL: {baseUrl}");
        }

        public async Task<FlagResult> GetFlagAsync(string userKey, string flagName, Dictionary<string, object>? attributes = null)
        {
            try
            {
                var flagUrl = string.Format(_flagUrlTemplate, userKey, flagName);

                if (attributes != null && attributes.Count > 0)
                {
                    var jsonAttr = JsonSerializer.Serialize(attributes);
                    var encodedAttr = Uri.EscapeDataString(jsonAttr);
                    flagUrl += $"&attributes={encodedAttr}";
                    Console.WriteLine($"Appended attributes: {jsonAttr}");
                }

                Console.WriteLine($"Constructed flag URL: {flagUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, flagUrl);
                request.Headers.Add("Authorization", "splitInTheW1ld!");
                Console.WriteLine("Added Authorization header.");

                var response = await _httpClient.SendAsync(request);
                Console.WriteLine($"HTTP status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Raw JSON response: {jsonString}");

                    var jsonDoc = JsonDocument.Parse(jsonString);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty(flagName, out var flag))
                    {
                        var treatment = flag.GetProperty("treatment").GetString() ?? "control";
                        Console.WriteLine($"Treatment value: {treatment}");

                        JsonElement? configElement = null;
                        if (flag.TryGetProperty("config", out var configRaw))
                        {
                            var configJson = configRaw.GetString();

                            if (!string.IsNullOrEmpty(configJson))
                            {
                                var configDoc = JsonDocument.Parse(configJson);
                                configElement = configDoc.RootElement.Clone();
                                Console.WriteLine("Parsed config JSON.");
                            }
                            else
                            {
                                Console.WriteLine("Config JSON was null or empty.");
                            }
                        }

                        return new FlagResult
                        {
                            Treatment = treatment,
                            Config = configElement
                        };
                    }
                    else
                    {
                        Console.WriteLine($"Flag '{flagName}' not found in response.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed HTTP response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetFlagAsync: {ex.Message}");
            }

            return new FlagResult { Treatment = "control", Config = null };
        }

        public async Task<string> GetGreetingAsync(string userKey, Dictionary<string, object>? attributes = null, string defaultGreeting = "Hello, Earthlings")
        {
            var flagName = "blazor_hello";
            var result = await GetFlagAsync(userKey, flagName, attributes);

            if (result.Config.HasValue && result.Config.Value.TryGetProperty("greeting", out var greetingElement))
            {
                var greeting = greetingElement.GetString();
                if (!string.IsNullOrEmpty(greeting))
                {
                    Console.WriteLine($"Extracted greeting: {greeting}");
                    return greeting;
                }
            }

            Console.WriteLine($"No greeting found in config. Using default: {defaultGreeting}");
            return defaultGreeting;
        }
    }
}

