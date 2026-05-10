namespace Mechanics.Application.Vehicles.Responses;

public class VehicleResponse
{
    public required Guid Id { get; init; }
    public required string Manufacturer { get; init; }
    public required string Model { get; init; }
    public required string Color { get; init; }
    public required string Year { get; init; }
    public required string LicensePlate { get; init; }
    public required string Chassis { get; init; }
    public required Guid OwnerId { get; init; }
}
