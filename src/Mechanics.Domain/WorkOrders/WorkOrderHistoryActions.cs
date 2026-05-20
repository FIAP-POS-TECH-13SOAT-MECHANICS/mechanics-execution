namespace Mechanics.Domain.WorkOrders;

public static class WorkOrderHistoryActions
{
    public const string BudgetExpired = nameof(BudgetExpired);
    public const string BudgetCreated = nameof(BudgetCreated);
    public const string BudgetApproved = nameof(BudgetApproved);
    public const string BudgetRejected = nameof(BudgetRejected);
    public const string StatusChanged = nameof(StatusChanged);
    public const string Assigned = nameof(Assigned);
}
