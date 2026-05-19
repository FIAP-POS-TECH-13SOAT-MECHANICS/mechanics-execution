using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendWorkOrderCreated(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default);

    Task SendWorkOrderPendingApproval(Customer customer, WorkOrder workOrder, Budget budget,
        CancellationToken cancellationToken = default);

    Task SendWorkOrderStatusChanged(Customer customer, WorkOrder workOrder, WorkOrderStatus previousStatus,
        CancellationToken cancellationToken = default);

    Task SendWorkOrderCancelled(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default);

    Task SendMechanicBudgetDecision(User mechanic, WorkOrder workOrder, Budget budget, bool approved,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia pesquisa pós-entrega para o cliente associada à ordem de serviço.
    /// </summary>
    /// <remarks>
    /// AVISO: lógica de envio ainda não implementada. Seguir fluxo do Event Storming de Notificações;
    /// </remarks>
    Task SendWorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default);


    Task SendUserPasswordCreationCode(User user, string passwordCreationCode, CancellationToken cancellationToken = default);
    Task UserPasswordChanged(User user, CancellationToken cancellationToken = default);

    Task SendCustomerUserPasswordCreationCode(User entity, string passwordCreationCode,
        CancellationToken cancellationToken = default);
}
