using Mechanics.Application.WorkOrdersApi.Responses;

namespace Mechanics.Application.WorkOrdersApi.Services;

public interface IWorkOrdersApiService
{
    Task<VehicleResponse?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
