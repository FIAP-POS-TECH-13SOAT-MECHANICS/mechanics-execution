using Mechanics.Infra.Security.Models;
using Mechanics.Tests.Behavior.Hooks;

namespace Mechanics.Tests.Behavior.Drivers;

public class ProductDriver
{
    private readonly HttpClient _client = ApiHook.Factory.GetAuthenticatedClient(RoleNames.Attendant);

    public async Task<HttpResponseMessage> ListAsync(int page = 1, int itemsPerPage = 10)
        => await _client.GetAsync($"/products?Page={page}&ItemsPerPage={itemsPerPage}");
}
