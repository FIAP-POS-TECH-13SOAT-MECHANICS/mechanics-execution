using Mechanics.Application.Budgets.Events;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Tests.WorkOrders;

[TestClass]
public class WorkOrderAppServicePublishTests
{
    [TestMethod]
    public async Task RequestApproval_Publishes_BudgetCreatedEvent()
    {
        // Arrange
        var service = ServicesCatalogMocks.CreateService(Guid.NewGuid());
        var workOrder = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), service);

        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.WorkOrders.Add(workOrder)).Build();

        var mapper = AutoMapperFactory.CreateMap("WorkOrders");
        var logger = new NullLogger<Mechanics.Application.WorkOrders.Services.WorkOrderAppService>();
        var identityApi = new IdentityApiServiceMock();
        var workOrdersApi = new WorkOrdersApiServiceMock();
        var publisher = new TestEventPublisher();

        var app = new Mechanics.Application.WorkOrders.Services.WorkOrderAppService(
            context, mapper, logger, identityApi, workOrdersApi, publisher);

        // Act
        await app.RequestApproval(workOrder.Id, Guid.NewGuid());

        // Assert
        var published = publisher.GetPublished<BudgetCreatedEvent>().ToList();
        Assert.IsTrue(published.Any(), "No BudgetCreatedEvent was published");
        var evt = published.First();
        Assert.AreEqual(workOrder.Id, evt.WorkOrderId);
        Assert.AreEqual(service.BasePrice, evt.Total);
        Assert.IsTrue(evt.Items.Any(), "Budget items should not be empty");
    }
}
