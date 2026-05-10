using Mechanics.Application.Vehicles.Responses;

namespace Mechanics.Application.Vehicles.Services;

public interface IWorkOrdersApiService
{
    Task<VehicleResponse?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
