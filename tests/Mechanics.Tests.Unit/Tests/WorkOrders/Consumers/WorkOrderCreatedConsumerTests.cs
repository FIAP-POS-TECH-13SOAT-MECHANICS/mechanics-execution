using Mechanics.Application.WorkOrders.Events;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Infra.Messaging.Consumers;
using Mechanics.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Tests.WorkOrders.Consumers;

[TestClass]
public class WorkOrderCreatedConsumerTests
{
    [TestMethod]
    public async Task Consume_CallsWorkOrderCreate()
    {
        var called = false;
        var fake = new FakeWorkOrderService(() => called = true);
        var consumer = new Mechanics.Application.WorkOrders.Consumers.WorkOrderCreatedConsumer(new NullLogger<Mechanics.Application.WorkOrders.Consumers.WorkOrderCreatedConsumer>(), fake);

        var evt = new WorkOrderCreatedEvent { WorkOrderId = Guid.NewGuid(), CustomerId = Guid.NewGuid(), VehicleId = Guid.NewGuid(), EventId = Guid.NewGuid(), OccurredAt = DateTime.Now, Status = "Received" };
        await consumer.ConsumeAsync(evt);

        Assert.IsTrue(called);
    }

    private class FakeWorkOrderService : IWorkOrderAppService
    {
        private readonly Action _called;
        public FakeWorkOrderService(Action called) => _called = called;
        public Task<Mechanics.Application.Utils.CommonResponses.CreateItemResponse> Create(WorkOrderCreatedEvent request, CancellationToken cancellationToken = default)
        {
            _called.Invoke();
            return Task.FromResult(new Mechanics.Application.Utils.CommonResponses.CreateItemResponse { CreatedId = request.WorkOrderId });
        }
    }
}
