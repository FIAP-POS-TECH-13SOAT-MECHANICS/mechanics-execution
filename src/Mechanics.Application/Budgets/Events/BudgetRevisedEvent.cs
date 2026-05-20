namespace Mechanics.Application.Budgets.Events;

public record BudgetRevisedEvent
{
    public required Guid WorkOrderId { get; init; }
    public required bool Approved { get; init; }
    public string? Notes { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
