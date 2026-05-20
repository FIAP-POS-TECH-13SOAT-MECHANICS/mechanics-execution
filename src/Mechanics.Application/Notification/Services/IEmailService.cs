using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Identity.Responses;
using Mechanics.Domain.Budgets;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendApprovedBudget(UserResponse mechanic, WorkOrder workOrder, Budget budget, BudgetRevisedEvent message,
        CancellationToken cancellationToken = default);

    Task SendExpiredBudget(UserResponse mechanic, WorkOrder wo, CancellationToken cancellationToken = default);

    Task SendRejectedBudget(UserResponse mechanic, WorkOrder wo, string? notes, CancellationToken cancellationToken = default);

    Task SendAssignmentEmail(WorkOrder wo, UserResponse assignedUser, string? comment,
        CancellationToken cancellationToken = default);
}
