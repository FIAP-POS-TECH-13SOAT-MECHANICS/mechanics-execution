using Mechanics.Application.Vehicles.Responses;

namespace Mechanics.Tests.Unit.Mocks;

public static class VehicleMocks
{
    public static VehicleResponse CreateVehicle(Guid id, Guid ownerId, string plate = "ABC1D23", string chassis = "9BW8ZZ377VT004251") => new()
    {
        Id = id,
        Manufacturer = "Volkswagen",
        Model = "Gol",
        Color = "silver",
        Year = "2019",
        LicensePlate = plate,
        Chassis = chassis,
        OwnerId = ownerId,
    };
}
