using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Vehicles.Requests;

public class GetVehiclesRequest : PaginatedListRequest
{
    /// <summary>
    ///     Filtro por ID do proprietário (Customer).
    /// </summary>
    public Guid? OwnerId { get; init; }

    /// <summary>
    ///     Filtro por número do chassi (ou parte).
    /// </summary>
    public string? Chassis { get; init; }

    /// <summary>
    ///     Filtro por placa (com ou sem separadores).
    /// </summary>
    public string? LicensePlate { get; init; }
}
