using Mechanics.Domain.Vehicles;

namespace Mechanics.Application.Vehicles.Responses;

public class GetVehicleResponse
{
    public required Guid Id { get; init; }
    public required string Manufacturer { get; init; }
    public required string Model { get; init; }
    public required VehicleColor Color { get; init; }
    public required string Year { get; init; }
    public required string LicensePlate { get; init; }
    public required string Chassis { get; init; }
    public required Guid OwnerId { get; init; }
}
