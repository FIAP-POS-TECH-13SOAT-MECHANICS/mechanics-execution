using Mechanics.Domain.Budgets;

namespace Mechanics.Tests.Unit.Mocks;

public static class BudgetMocks
{
    public static Budget CreateBudget(Guid id, Guid workOrderId, BudgetStatus status = BudgetStatus.Sent, DateTime? creationDate = null)
    {
        var date = creationDate ?? DateTime.Now;
        return new Budget
        {
            Id = id,
            WorkOrderId = workOrderId,
            Status = status,
            CreationDate = date,
            Items = new List<BudgetItem>(),
            ApprovedAt = status == BudgetStatus.Approved ? date.AddHours(1) : null
        };
    }
}
