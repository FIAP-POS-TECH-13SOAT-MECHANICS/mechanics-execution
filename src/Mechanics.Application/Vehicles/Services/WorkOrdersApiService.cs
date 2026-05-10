using Mechanics.Application.Vehicles.Responses;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Application.Vehicles.Services;

public class WorkOrdersApiService(HttpClient client) : IWorkOrdersApiService
{
    public async Task<VehicleResponse?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync($"work-orders/vehicles/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VehicleResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync($"work-orders/customers/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: cancellationToken);
    }
}
