using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders.Requests;

public class GetWorkOrdersRequest : PaginatedListRequest
{
    /// <summary>
    ///     Filtro por ID do cliente (Customer).
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    ///     Filtro por ID do veículo.
    /// </summary>
    public Guid? VehicleId { get; init; }

    /// <summary>
    ///     Filtro por data.
    /// </summary>
    public DateTime? DateOfTheDay { get; init; }

    /// <summary>
    ///     Incluir OSs com status <see cref="WorkOrderStatus.Completed"/> ou <see cref="WorkOrderStatus.Delivered"/>.
    /// </summary>
    public bool IncludeCompleted { get; init; }
}
