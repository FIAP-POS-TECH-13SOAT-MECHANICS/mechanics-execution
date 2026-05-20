using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Budgets.Consumers;

public abstract class BudgetRevisedConsumer(ILogger<BudgetRevisedConsumer> logger, BudgetAppService service) : IEventConsumer<BudgetRevisedEvent>
{
    public async Task ConsumeAsync(BudgetRevisedEvent message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing revised budget for work order '{WorkOrderId}'", message.WorkOrderId);

        await service.UpdateBudget(message, cancellationToken);

        logger.LogInformation("Revised budget for work order '{WorkOrderId}' processed", message.WorkOrderId);
    }
}
