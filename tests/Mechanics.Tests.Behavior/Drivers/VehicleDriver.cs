using Mechanics.Domain.Auth;
using Mechanics.Tests.Behavior.Hooks;

namespace Mechanics.Tests.Behavior.Drivers;

public class VehicleDriver
{
    private readonly HttpClient _client = ApiHook.Factory.GetAuthenticatedClient(RoleNames.Attendant);

    public async Task<HttpResponseMessage> ListAsync(int page = 1, int itemsPerPage = 10)
        => await _client.GetAsync($"/api/vehicles?Page={page}&ItemsPerPage={itemsPerPage}");
}
