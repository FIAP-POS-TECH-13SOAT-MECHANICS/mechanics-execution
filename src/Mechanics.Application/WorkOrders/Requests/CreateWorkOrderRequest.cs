namespace Mechanics.Application.WorkOrders.Requests;

public class CreateWorkOrderRequest
{
    public required Guid VehicleId { get; init; }
    public string? ReportedProblem { get; init; }
    public IEnumerable<WorkOrderProductRequest>? Products { get; init; }
    public IEnumerable<Guid>? ServiceCatalogIds { get; init; }
}

public class WorkOrderProductRequest
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}
