using Mechanics.Application.WorkOrdersApi.Responses;
using Mechanics.Application.WorkOrdersApi.Services;

namespace Mechanics.Tests.Unit.Mocks;

public class WorkOrdersApiServiceMock : IWorkOrdersApiService
{
    public VehicleResponse? VehicleToReturn { get; set; }
    public CustomerResponse? CustomerToReturn { get; set; }

    public Task<VehicleResponse?> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (VehicleToReturn == null)
            return Task.FromResult<VehicleResponse?>(null);

        if (VehicleToReturn != null && VehicleToReturn.Id == id)
            return Task.FromResult<VehicleResponse?>(VehicleToReturn);

        return Task.FromResult<VehicleResponse?>(new VehicleResponse
        {
            Id = id,
            Manufacturer = "Mock Manufacturer",
            Model = "Mock Model",
            Color = "Mock Color",
            Year = "2024",
            LicensePlate = "ABC1D23",
            Chassis = "1234567890",
            OwnerId = Guid.NewGuid(),
        });
    }

    public Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (CustomerToReturn != null && CustomerToReturn.Id == id)
            return Task.FromResult<CustomerResponse?>(CustomerToReturn);

        if (CustomerToReturn == null)
            return Task.FromResult<CustomerResponse?>(null);

        return Task.FromResult<CustomerResponse?>(new CustomerResponse
        {
            Id = id,
            Name = "Mock Customer",
            Email = "mock@customer.com",
            Document = new PersonalDocumentResponse
            {
                Type = "CPF",
                Number = "12345678901",
            },
        });
    }
}
