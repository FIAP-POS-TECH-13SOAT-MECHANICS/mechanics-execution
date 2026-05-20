namespace Mechanics.Application.Budgets.Events;

public record BudgetCreatedEvent
{
    public required Guid EventId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid VehicleId { get; init; }
    public required decimal Total { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IEnumerable<BudgetItem> Items { get; init; }
}

public record BudgetItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int Quantity { get; init; }
    public required decimal Subtotal { get; init; }
}

