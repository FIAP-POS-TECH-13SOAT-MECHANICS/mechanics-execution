using Mechanics.Domain.Vehicles;

namespace Mechanics.Application.Vehicles.Requests;

public class CreateVehicleRequest
{
    /// <summary>
    ///     Fabricante do veículo.
    /// </summary>
    public required string Manufacturer { get; init; }

    /// <summary>
    ///     Modelo do veículo.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    ///     Cor do veículo.
    /// </summary>
    public VehicleColor? Color { get; init; }

    /// <summary>
    ///     Ano de fabricação/modelo. Aceita quatro dígitos.
    /// </summary>
    /// <example>2020</example>
    public required string Year { get; init; }

    /// <summary>
    ///     Placa do veículo no padrão nacional ou Mercosul.
    /// </summary>
    /// <example>ABC1D23</example>
    public required string LicensePlate { get; init; }

    /// <summary>
    ///     Número do chassi do veículo.
    /// </summary>
    public required string Chassis { get; init; }

    /// <summary>
    ///     ID do proprietário (cliente) deste veículo.
    /// </summary>
    public required Guid OwnerId { get; init; }
}
