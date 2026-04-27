using Mechanics.Application.Vehicles.Requests;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Tests.Unit.Mocks;

public static class VehicleMocks
{
    public static CreateVehicleRequest BuildCreateRequest(Guid ownerId) => new()
    {
        Manufacturer = "Ford",
        Model = "Fiesta",
        Color = VehicleColor.Black,
        Year = "2020",
        LicensePlate =  "ABC1D23",
        Chassis =  "9BWZZZ377VT004251",
        OwnerId = ownerId,
    };

    public static CreateVehicleRequest BuildInvalidPlateCreateRequest(Guid ownerId) => new()
    {
        Manufacturer = "Ford",
        Model = "Fiesta",
        Color = VehicleColor.Black,
        Year = "2020",
        LicensePlate = "INVALID!",
        Chassis = "9BW8ZZ377VT004251",
        OwnerId = ownerId,
    };

    public static UpdateVehicleRequest BuildUpdateRequest(Guid? newOwnerId = null) => new()
    {
        Manufacturer = "Chevrolet",
        Model = "Onix",
        Color = VehicleColor.White,
        Year = "2021",
        LicensePlate = "DEF2G34",
        Chassis = "9BW8ZZ377VT004252",
        OwnerId = newOwnerId,
    };

    public static Vehicle CreateVehicle(Guid id, Guid ownerId, string plate = "ABC1D23", string chassis = "9BW8ZZ377VT004251") => new()
    {
        Id = id,
        Manufacturer = "Volkswagen",
        Model = "Gol",
        Color = VehicleColor.Silver,
        Year = "2019",
        LicensePlate = new LicensePlate(plate),
        Chassis = chassis,
        OwnerId = ownerId,
        Owner = null,
    };
}
