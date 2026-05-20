using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Identity.Services;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Observability;
using Mechanics.Application.Utils;
using Mechanics.Application.WorkOrders.Events;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Budgets;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Mechanics.Infra.Messaging.Publishers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mechanics.Application.Budgets.Services;

/// <summary>
///     Serviço para criação, envio e aprovação pública de budgets.
/// </summary>
public class BudgetAppService(
    AppDbContext dbContext,
    IEmailService emailService,
    IEventPublisher eventPublisher,
    IIdentityApiService identityApiService,
    ILogger<BudgetAppService> logger) : IAppService
{
    /// <summary>
    ///     Processa a resposta a um orçamento.
    /// </summary>
    public async Task UpdateBudget(BudgetRevisedEvent message, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == message.WorkOrderId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, message.WorkOrderId);

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);
        EntityNotFoundException.ThrowIfNull(budget, wo.Id);

        if (message.Approved && budget.ApprovedAt != null)
        {
            logger.LogWarning("Budget '{BudgetId}' already approved for work order '{WorkOrderId}'", budget.Id, wo.Id);
            return;
        }

        var mechanic = await identityApiService.GetUserById(wo.AssignedToUserId!.Value, cancellationToken);
        EntityNotFoundException.ThrowIfNull(mechanic, wo.AssignedToUserId);

        if (!message.Approved && DateTime.Now > budget.ExpiresAt)
        {
            budget.Status = BudgetStatus.Expired;
            AddWorkOrderHistory(message.WorkOrderId, WorkOrderHistoryActions.BudgetExpired, message.Notes);
            await ReleaseReservedProducts(wo.Id, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await emailService.SendExpiredBudget(mechanic, wo, cancellationToken);

            logger.LogInformation("Budget '{BudgetId}' expired for work order '{WorkOrderId}'", budget.Id, wo.Id);
            return;
        }

        if (!message.Approved)
        {
            budget.Status = BudgetStatus.Rejected;
            AddWorkOrderHistory(message.WorkOrderId, WorkOrderHistoryActions.BudgetRejected, message.Notes);
            await ReleaseReservedProducts(wo.Id, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await emailService.SendRejectedBudget(mechanic, wo, message.Notes, cancellationToken);

            logger.LogInformation("Budget '{BudgetId}' rejected for work order '{WorkOrderId}'", budget.Id, wo.Id);
            return;
        }

        var previousStatus = wo.Status;
        var timeInPreviousStatus = message.OccurredAt.DateTime - wo.LastUpdate;

        budget.Status = BudgetStatus.Approved;
        budget.ApprovedAt = message.OccurredAt.DateTime;
        budget.CustomerNotes = message.Notes;

        wo.Status = WorkOrderStatus.InProgress;
        wo.LastUpdate = DateTime.Now;

        var tags = new TagList
        {
            { "previous_status", previousStatus.ToString() },
            { "new_status", nameof(WorkOrderStatus.InProgress) },
        };

        AppMetrics.TimeInStatusTotalSeconds.Add(Math.Round(timeInPreviousStatus.TotalSeconds, 2), tags);
        AppMetrics.TimeInStatusSamples.Add(1, tags);
        AppMetrics.StatusTransitions.Add(1, tags);

        AppMetrics.StatusDurationSeconds.Record(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            new TagList { { "status", previousStatus.ToString() } });

        AddWorkOrderHistory(wo.Id, WorkOrderHistoryActions.BudgetApproved, message.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publica evento de status
        await eventPublisher.PublishAsync(new WorkOrderStatusChangedEvent
        {
            WorkOrderId = wo.Id,
            LastStatusChangeBy = Guid.Empty,
            OldStatus = previousStatus.ToString(),
            NewStatus = wo.Status.ToString(),
            LastUpdate = DateTimeOffset.Now,
        }, cancellationToken);

        // Notificação ao mecânico
        try
        {
            await emailService.SendApprovedBudget(mechanic, wo, budget, message, cancellationToken);
            AppMetrics.EmailsSent.Add(1, new TagList { { "template", "budget_approved_async_mechanic" } });
        }
        catch (Exception ex)
        {
            AppMetrics.EmailsFailed.Add(1, new TagList { { "template", "budget_approved_async_mechanic" } });
            logger.LogWarning(ex, "Failed to send mechanic notification for approved budget {BudgetId}", budget.Id);
        }
    }

    private async Task ReleaseReservedProducts(Guid workOrderId, CancellationToken cancellationToken)
    {
        var wo = await dbContext.WorkOrders
            .Where(wo => wo.Id == workOrderId)
            .Include(wo => wo.Products)!
            .ThenInclude(product => product.Product)
            .FirstAsync(cancellationToken: cancellationToken);

        foreach (var products in wo.Products!)
            products.Product!.Quantity += products.Quantity;
    }

    private void AddWorkOrderHistory(Guid workOrderId, string action, string? details)
    {
        var history = new WorkOrderHistory
        {
            WorkOrderId = workOrderId,
            Action = action,
            Details = details,
        };

        dbContext.WorkOrderHistories.Add(history);
    }
}
