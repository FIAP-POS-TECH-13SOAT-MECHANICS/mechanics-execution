using Mechanics.Domain.Base;

namespace Mechanics.Domain.WorkOrders;

public class WorkOrderHistory : AbstractEntity
{
    public required Guid WorkOrderId { get; init; }
    public WorkOrder? WorkOrder { get; init; }
    public required string Action { get; init; }
    public string? Details { get; init; }
    public Guid? PerformedByUserId { get; init; }
}
