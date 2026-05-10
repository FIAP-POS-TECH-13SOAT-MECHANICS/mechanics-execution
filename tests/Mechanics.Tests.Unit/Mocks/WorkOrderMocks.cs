using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public static class WorkOrderMocks
{
    public static WorkOrder CreateWorkOrderEntity(Guid id, Guid customerId, Guid vehicleId, Guid? assignedToUserId = null)
    {
        var now = DateTime.Now;
        var userId = assignedToUserId ?? new Guid("380038b3-5118-484a-bfd3-35df9363d969");
        return new WorkOrder
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = "21832403",
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            AssignedToUserId = userId,
        };
    }

    public static WorkOrder CreateWorkOrderEntity(WorkOrderStatus status)
    {
        var workOrder = CreateWorkOrderEntity(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        workOrder.Status = status;

        return workOrder;
    }

    public static WorkOrder CreateWorkOrderWithServices(Guid id, Guid customerId, Guid vehicleId, ServiceCatalog service)
    {
        var now = DateTime.Now;
        return new WorkOrder
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = "53861453",
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            ServiceCatalog = new List<ServiceCatalog> { service },
            AssignedToUserId = new Guid("380038b3-5118-484a-bfd3-35df9363d969"),
        };
    }
}
