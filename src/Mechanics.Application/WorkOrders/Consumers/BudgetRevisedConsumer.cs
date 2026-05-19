using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.WorkOrders.Consumers;

/// <summary>
/// Consumer para o evento BudgetRevisedEvent publicado pelo serviço de Billing.
/// Processa aprovação ou rejeição do orçamento pela via de evento assíncrono.
/// </summary>
public class BudgetRevisedConsumer(
    ILogger<BudgetRevisedConsumer> logger,
    IBudgetAppService budgetAppService) : IEventConsumer<BudgetRevisedEvent>
{
    public async Task ConsumeAsync(BudgetRevisedEvent message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing budget revision for work order '{WorkOrderId}' - Approved: {Approved}",
            message.WorkOrderId,
            message.Approved);

        try
        {
            if (message.Approved)
            {
                // Orçamento aprovado pelo cliente
                logger.LogInformation(
                    "Budget approved for work order '{WorkOrderId}'",
                    message.WorkOrderId);

                await budgetAppService.ApproveBudget(
                    message.WorkOrderId,
                    message.Notes,
                    cancellationToken);
            }
            else
            {
                // Orçamento rejeitado ou expirado
                logger.LogInformation(
                    "Budget rejected or expired for work order '{WorkOrderId}'. Notes: {Notes}",
                    message.WorkOrderId,
                    message.Notes ?? "N/A");

                await budgetAppService.RejectBudget(
                    message.WorkOrderId,
                    message.Notes,
                    cancellationToken);
            }

            logger.LogInformation(
                "Budget revision processed successfully for work order '{WorkOrderId}'",
                message.WorkOrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing budget revision for work order '{WorkOrderId}'", message.WorkOrderId);
            throw; // Relança para SQS reprocessar
        }
    }
}
