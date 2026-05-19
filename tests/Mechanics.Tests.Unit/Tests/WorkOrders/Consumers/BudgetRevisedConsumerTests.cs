using Mechanics.Application.Budgets.Events;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Application.Budgets.Services;
using Mechanics.Infra.Messaging.Consumers;
using Mechanics.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Tests.WorkOrders.Consumers;

[TestClass]
public class BudgetRevisedConsumerTests
{
    [TestMethod]
    public async Task Consume_Approved_CallsApproveBudget()
    {
        var called = false;
        var fake = new FakeBudgetService(() => called = true);
        var consumer = new Mechanics.Application.WorkOrders.Consumers.BudgetRevisedConsumer(new NullLogger<Mechanics.Application.WorkOrders.Consumers.BudgetRevisedConsumer>(), fake);

        var evt = new BudgetRevisedEvent { WorkOrderId = Guid.NewGuid(), Approved = true, OccurredAt = DateTimeOffset.Now };
        await consumer.ConsumeAsync(evt);

        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task Consume_Rejected_CallsRejectBudget()
    {
        var called = false;
        var fake = new FakeBudgetService(rejectCalled: () => called = true);
        var consumer = new Mechanics.Application.WorkOrders.Consumers.BudgetRevisedConsumer(new NullLogger<Mechanics.Application.WorkOrders.Consumers.BudgetRevisedConsumer>(), fake);

        var evt = new BudgetRevisedEvent { WorkOrderId = Guid.NewGuid(), Approved = false, OccurredAt = DateTimeOffset.Now };
        await consumer.ConsumeAsync(evt);

        Assert.IsTrue(called);
    }

    private class FakeBudgetService : IBudgetAppService
    {
        private readonly Action? _approveCalled;
        private readonly Action? _rejectCalled;

        public FakeBudgetService(Action? approveCalled = null, Action? rejectCalled = null)
        {
            _approveCalled = approveCalled;
            _rejectCalled = rejectCalled;
        }

        public Task ApproveBudget(Guid workOrderId, string? description = null, CancellationToken cancellationToken = default)
        {
            _approveCalled?.Invoke();
            return Task.CompletedTask;
        }

        public Task RejectBudget(Guid workOrderId, string? description = null, CancellationToken cancellationToken = default)
        {
            _rejectCalled?.Invoke();
            return Task.CompletedTask;
        }
    }
}
