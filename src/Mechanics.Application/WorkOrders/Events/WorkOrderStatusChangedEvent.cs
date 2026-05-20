namespace Mechanics.Application.WorkOrders.Events;

public record WorkOrderStatusChangedEvent
{
    public required Guid WorkOrderId { get; init; }
    public required Guid LastStatusChangeBy { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
    public required DateTimeOffset LastUpdate { get; init; }
}
