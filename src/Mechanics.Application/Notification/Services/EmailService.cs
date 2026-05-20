using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Identity.Responses;
using Mechanics.Application.Notification.Templates;
using Mechanics.Domain.Budgets;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Notification.Services;

public class EmailService(ILogger<EmailService> logger, IEmailSenderService senderService) : IEmailService
{
    public async Task SendCustomerResponse(UserResponse mechanic, WorkOrder workOrder, Budget budget,
        BudgetRevisedEvent response, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending mechanic budget decision to '{EmailAddress}'", mechanic.Email);

        var message = BudgetEmailTemplates.CustomerResponse(mechanic, workOrder, budget, response);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Mechanic budget decision sent to '{EmailAddress}'", mechanic.Email);
    }

    public async Task SendExpiredBudget(UserResponse mechanic, WorkOrder wo, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending expired budget notification to '{EmailAddress}'", mechanic.Email);

        var message = BudgetEmailTemplates.ExpiredBudget(mechanic, wo);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Expired budget notification sent to '{EmailAddress}'", mechanic.Email);
    }

    public async Task SendAssignmentEmail(WorkOrder wo, UserResponse assignedUser, string? comment, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order assignment notification to '{EmailAddress}'", assignedUser.Email);

        var message = WorkOrderEmailTemplates.AssignmentEmail(wo, assignedUser, comment);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order assignment notification sent to '{EmailAddress}'", assignedUser.Email);
    }
}
