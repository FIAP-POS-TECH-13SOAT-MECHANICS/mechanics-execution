namespace Mechanics.Domain.WorkOrders;

public enum WorkOrderStatus
{
    Received,
    UnderDiagnosis,
    PendingApproval,
    InProgress,
    Completed,
    Delivered,
}
