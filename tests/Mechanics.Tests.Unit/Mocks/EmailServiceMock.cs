using Mechanics.Application.Notification.Services;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

/// <summary>
/// Spy para IEmailService, colocado na pasta Mocks para seguir o padrão do repositório.
/// Permite asserts simples nos testes unitários.
/// </summary>
public class EmailServiceMock : IEmailService
{
    public bool SendWorkOrderCreatedCalled { get; private set; }
    public bool SendWorkOrderPendingApprovalCalled { get; private set; }
    public bool SendWorkOrderStatusChangedCalled { get; private set; }
    public bool SendWorkOrderCancelledCalled { get; private set; }
    public bool SendWorkOrderDeliveredSurveyCalled { get; private set; }
    public string? LastPreviousStatus { get; private set; }
    public string? LastNewStatus { get; private set; }

    public Task SendWorkOrderCreated(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        SendWorkOrderCreatedCalled = true;
        return Task.CompletedTask;
    }

    public Task SendWorkOrderStatusChanged(Customer customer, WorkOrder workOrder, WorkOrderStatus previousStatus,
        CancellationToken cancellationToken = default)
    {
        SendWorkOrderStatusChangedCalled = true;
        LastPreviousStatus = previousStatus.ToString();
        LastNewStatus = workOrder.Status.ToString();
        return Task.CompletedTask;
    }

    public Task SendWorkOrderCancelled(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        SendWorkOrderCancelledCalled = true;
        return Task.CompletedTask;
    }

    public Task SendWorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        SendWorkOrderDeliveredSurveyCalled = true;
        return Task.CompletedTask;
    }
}
