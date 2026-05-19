namespace Mechanics.Application.Budgets.Services;

public interface IBudgetAppService
{
    Task ApproveBudget(Guid workOrderId, string? description = null, CancellationToken cancellationToken = default);
    Task RejectBudget(Guid workOrderId, string? description = null, CancellationToken cancellationToken = default);
}
