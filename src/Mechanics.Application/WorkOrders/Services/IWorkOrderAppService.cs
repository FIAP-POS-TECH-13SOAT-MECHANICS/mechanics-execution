using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.WorkOrders.Events;

namespace Mechanics.Application.WorkOrders.Services;

public interface IWorkOrderAppService
{
    Task<CreateItemResponse> Create(WorkOrderCreatedEvent request, CancellationToken cancellationToken = default);
}
