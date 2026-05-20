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
    ///// <summary>
    /////     Cria um <see cref="Budget"/> a partir dos produtos/serviços atualmente associados à WorkOrder,
    /////     persiste snapshot de preços e itens, define ExpiresAt = CreationDate + 3 dias,
    /////     atualiza <see cref="WorkOrder.Status"/> para PendingApproval e registra o usuário responsável.
    ///// </summary>
    //public async Task CreateAndSendBudget(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    //{
    //    if (performedByUserId == Guid.Empty)
    //        throw new BusinessException("PerformedByUserId must be informed.");

    //    var wo = await dbContext.WorkOrders
    //        .Include(w => w.Products)
    //        .Include(w => w.ServiceCatalog)
    //        .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

    //    EntityNotFoundException.ThrowIfNull(wo, workOrderId);

    //    if ((wo.Products == null || wo.Products.Count == 0) && (wo.ServiceCatalog == null || wo.ServiceCatalog.Count == 0))
    //        throw new BusinessException("Order must contain at least one product or service to create a budget.");

    //    var now = DateTime.Now;
    //    var budget = new Budget
    //    {
    //        WorkOrderId = wo.Id,
    //        CreationDate = now,
    //        ExpiresAt = now.AddDays(3),
    //        Status = BudgetStatus.Sent,
    //        Items = new List<BudgetItem>(),
    //    };

    //    var partsTotal = 0m;
    //    if (wo.Products?.Count > 0)
    //    {
    //        var productIds = wo.Products.Select(p => p.ProductId).ToList();
    //        var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

    //        foreach (var workOrderProduct in wo.Products)
    //        {
    //            var product = products.First(product => product.Id == workOrderProduct.ProductId);
    //            var item = new BudgetItem
    //            {
    //                BudgetId = budget.Id,
    //                ProductId = product.Id,
    //                NameSnapshot = product.Name,
    //                UnitPriceSnapshot = product.UnitPrice,
    //                Quantity = workOrderProduct.Quantity,
    //                Subtotal = product.UnitPrice * workOrderProduct.Quantity,
    //            };
    //            partsTotal += item.Subtotal;
    //            budget.Items.Add(item);
    //        }
    //    }

    //    var servicesTotal = 0m;
    //    if (wo.ServiceCatalog?.Count > 0)
    //    {
    //        var serviceIds = wo.ServiceCatalog.Select(s => s.Id).ToList();
    //        var services = await dbContext.ServiceCatalog.Where(s => serviceIds.Contains(s.Id))
    //            .ToListAsync(cancellationToken);

    //        foreach (var item in services.Select(s => new BudgetItem
    //        {
    //            BudgetId = budget.Id,
    //            ServiceCatalogId = s.Id,
    //            NameSnapshot = s.Name,
    //            UnitPriceSnapshot = s.BasePrice,
    //            Quantity = 1, // serviços são individuais
    //            Subtotal = s.BasePrice,
    //        }))
    //        {
    //            servicesTotal += item.Subtotal;
    //            budget.Items.Add(item);
    //        }
    //    }

    //    budget.Total = partsTotal + servicesTotal;

    //    await dbContext.Budgets.AddAsync(budget, cancellationToken);

    //    var previousStatus = wo.Status;
    //    var timeInPreviousStatus = DateTime.Now - wo.LastUpdate;

    //    wo.ApprovalRequestedAt = now;
    //    wo.Status = WorkOrderStatus.PendingApproval;
    //    wo.LastStatusChangeBy ??= performedByUserId;
    //    wo.LastUpdate = now;

    //    var tags = new TagList
    //    {
    //        { "previous_status", previousStatus.ToString() },
    //        { "new_status", nameof(WorkOrderStatus.PendingApproval) },
    //    };

    //    AppMetrics.TimeInStatusTotalSeconds.Add(
    //        Math.Round(timeInPreviousStatus.TotalSeconds, 2), tags);
    //    AppMetrics.TimeInStatusSamples.Add(1, tags);
    //    AppMetrics.StatusTransitions.Add(1, tags);

    //    AppMetrics.StatusDurationSeconds.Record(
    //        Math.Round(timeInPreviousStatus.TotalSeconds, 2),
    //        new TagList { { "status", previousStatus.ToString() } });

    //    await dbContext.SaveChangesAsync(cancellationToken);

    //    var hist = new WorkOrderHistory
    //    {
    //        WorkOrderId = wo.Id,
    //        Action = "BudgetSent",
    //        Details = $"Budget {budget.Id} sent. Total: {budget.Total:C}",
    //        PerformedByUserId = performedByUserId,
    //    };
    //    await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

    //    await dbContext.SaveChangesAsync(cancellationToken);

    //    var customer = await dbContext.Customers.FindAsync([wo.CustomerId], cancellationToken);
    //    if (customer == null)
    //        return;

    //    try
    //    {
    //        await emailService.SendWorkOrderPendingApproval(customer, wo, budget, cancellationToken);
    //        AppMetrics.EmailsSent.Add(1, new TagList
    //        {
    //            { "template", "budget_pending_approval" }
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        AppMetrics.EmailsFailed.Add(1, new TagList
    //        {
    //            { "template", "budget_pending_approval" }
    //        });
    //        logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", wo.Id);
    //    }
    //}

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
        if (wo.AssignedToUserId != null)
        {
            try
            {
                await emailService.SendCustomerResponse(mechanic, wo, budget, message, cancellationToken);
                AppMetrics.EmailsSent.Add(1, new TagList { { "template", "budget_approved_async_mechanic" } });
            }
            catch (Exception ex)
            {
                AppMetrics.EmailsFailed.Add(1, new TagList { { "template", "budget_approved_async_mechanic" } });
                logger.LogWarning(ex, "Failed to send mechanic notification for approved budget {BudgetId}", budget.Id);
            }
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
            products.Product!.Quantity -= products.Quantity;
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
