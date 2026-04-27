using Mechanics.Domain.Vehicles;

namespace Mechanics.Application.Vehicles.Requests;

public class UpdateVehicleRequest
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Fabricante do veículo. Opcional para atualização.
    /// </summary>
    public string? Manufacturer { get; init; }

    /// <summary>
    ///     Modelo do veículo. Opcional para atualização.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    ///     Cor do veículo. Opcional para atualização.
    /// </summary>
    public VehicleColor? Color { get; init; }

    /// <summary>
    ///     Ano de fabricação/modelo (YYYY). Opcional para atualização.
    /// </summary>
    public string? Year { get; init; }

    /// <summary>
    ///     Placa do veículo no padrão nacional (7 caracteres sem separadores). Opcional para atualização.
    /// </summary>
    public string? LicensePlate { get; init; }

    /// <summary>
    ///     Número do chassi do veículo. Opcional para atualização.
    /// </summary>
    public string? Chassis { get; init; }

    /// <summary>
    ///     ID do proprietário (Customer) deste veículo. Opcional para atualização.
    /// </summary>
    public Guid? OwnerId { get; init; }
}
