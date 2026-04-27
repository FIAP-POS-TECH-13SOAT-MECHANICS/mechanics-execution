using Mechanics.Domain.Auth;
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
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            AssignedToUser = UserMocks.CreateUser(userId, $"meca-{userId:N}", "63196372006", RoleNames.Mechanic),
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
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            ServiceCatalog = new List<ServiceCatalog> { service },
            AssignedToUser = UserMocks.CreateUser(new Guid("380038b3-5118-484a-bfd3-35df9363d969"), "meca", "77184822005",
                RoleNames.Mechanic),
            AssignedToUserId = new Guid("380038b3-5118-484a-bfd3-35df9363d969"),
        };
    }
}
