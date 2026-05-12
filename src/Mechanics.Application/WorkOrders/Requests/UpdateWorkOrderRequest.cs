namespace Mechanics.Application.WorkOrders.Requests;

public class UpdateWorkOrderRequest
{
    public IEnumerable<WorkOrderProductRequest>? Products { get; init; }
    public IEnumerable<Guid>? ServiceIds { get; init; }
    public string? Observations { get; init; }
}

public class WorkOrderProductRequest
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}
