namespace TodoApi.ApiTests;

/// <summary>
/// Configuration for API tests.
/// Tests run against a real running API instance (acceptance environment).
/// </summary>
public static class ApiTestConfiguration
{
    /// <summary>
    /// Base URL of the API to test against.
    /// Can be overridden via environment variable API_BASE_URL.
    /// Default: http://localhost:5000 (docker-compose environment)
    /// </summary>
    public static string BaseUrl => 
        Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
    
    /// <summary>
    /// Creates an HttpClient configured to test against the API.
    /// </summary>
    public static HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }
}




