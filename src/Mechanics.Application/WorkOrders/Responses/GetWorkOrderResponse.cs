using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders.Responses;

public class GetWorkOrdersResponse(IEnumerable<GetWorkOrderResponse> items, int totalCount)
    : PaginatedListResponse<GetWorkOrderResponse>(items, totalCount);

public class GetWorkOrderResponse
{
    public required Guid Id { get; init; }
    public required string AccessKey { get; init; }
    public required WorkOrderStatus Status { get; init; }
    public required DateTime CreationDate { get; init; }
    public DateTime? LastUpdate { get; init; }
    public Guid CustomerId { get; init; }
    public Guid VehicleId { get; init; }

    public string? ReportedProblem { get; init; }
    public string? Observations { get; init; }

    public IEnumerable<WorkOrderProductResponse>? Products { get; init; }
    public IEnumerable<Guid>? ServiceCatalogIds { get; init; }
}

public class WorkOrderProductResponse
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
