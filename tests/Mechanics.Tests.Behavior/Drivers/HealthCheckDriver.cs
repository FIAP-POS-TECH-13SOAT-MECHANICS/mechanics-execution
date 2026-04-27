using Mechanics.Tests.Behavior.Hooks;

namespace Mechanics.Tests.Behavior.Drivers;

public class HealthCheckDriver
{
    private readonly HttpClient _client = ApiHook.Factory.CreateClient();

    public async Task<HttpResponseMessage> GetHealthAsync()
        => await _client.GetAsync("/api/health");
}
