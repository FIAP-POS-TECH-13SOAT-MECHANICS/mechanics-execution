using Mechanics.Application.Identity.Responses;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Application.Identity.Services;

public class IdentityApiService(HttpClient client) : IIdentityApiService
{
    public async Task<UserResponse?> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"identity/users/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellationToken);
    }
}
