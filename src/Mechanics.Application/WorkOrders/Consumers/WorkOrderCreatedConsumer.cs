using Mechanics.Application.WorkOrders.Events;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.WorkOrders.Consumers;

public class WorkOrderCreatedConsumer(ILogger<WorkOrderCreatedConsumer> logger, IWorkOrderAppService service) : IEventConsumer<WorkOrderCreatedEvent>
{
    public async Task ConsumeAsync(WorkOrderCreatedEvent message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating work order for customer '{CustomerId}'", message.CustomerId);

        var response = await service.Create(message, cancellationToken);

        logger.LogInformation("Work order for customer '{CustomerId}' created with ID '{WorkOrderId}'",
            message.CustomerId, response.CreatedId);
    }
}
