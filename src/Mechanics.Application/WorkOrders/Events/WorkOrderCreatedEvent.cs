namespace Mechanics.Application.WorkOrders.Events;

public class WorkOrderCreatedEvent
{
    public required Guid EventId { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required string AccessKey { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid VehicleId { get; init; }
    public required string Status { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public string? ReportedProblem { get; init; }
}
