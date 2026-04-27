using Mechanics.Application.Notification.Templates;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Notification.Services;

public class EmailService(ILogger<EmailService> logger, IEmailSenderService senderService) : IEmailService
{
    public async Task SendWorkOrderCreated(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order created notification to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderCreated(customer, workOrder);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order created notification sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendWorkOrderPendingApproval(Customer customer, WorkOrder workOrder, Budget budget,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order pending approval to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderPendingApproval(customer, workOrder, budget);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order pending approval sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendWorkOrderStatusChanged(Customer customer, WorkOrder workOrder, WorkOrderStatus previousStatus,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order status changed to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderStatusChanged(customer, workOrder, previousStatus);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order status changed sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendWorkOrderCancelled(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order cancelled to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderCancelled(customer, workOrder);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order cancelled sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendMechanicBudgetDecision(User mechanic, WorkOrder workOrder, Budget budget, bool approved,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending mechanic budget decision to '{EmailAddress}'", mechanic.Email);

        var message = WorkOrderEmailTemplates.MechanicBudgetDecision(mechanic, workOrder, budget, approved);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Mechanic budget decision sent to '{EmailAddress}'", mechanic.Email);
    }

    public async Task SendWorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order delivered survey to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderDeliveredSurvey(customer, workOrder);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order delivered survey sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendUserPasswordCreationCode(User user, string passwordCreationCode,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending password creation code to '{EmailAddress}'", user.Email);

        var message = AuthEmailTemplates.UserPasswordCreationCode(user, passwordCreationCode);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Password creation code sent to '{EmailAddress}'", user.Email);
    }

    public async Task UserPasswordChanged(User user, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending password changed notification to '{EmailAddress}'", user.Email);

        var message = AuthEmailTemplates.UserPasswordChanged(user);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Password changed notification sent to '{EmailAddress}'", user.Email);
    }

    public async Task SendCustomerUserPasswordCreationCode(User user, string passwordCreationCode,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending customer password creation code to '{EmailAddress}'", user.Email);

        var message = AuthEmailTemplates.CustomerUserPasswordCreationCode(user, passwordCreationCode);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Customer password creation code sent to '{EmailAddress}'", user.Email);
    }
}
