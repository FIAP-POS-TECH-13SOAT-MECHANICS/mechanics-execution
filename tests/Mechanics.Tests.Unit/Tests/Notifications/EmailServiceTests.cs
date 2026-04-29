using Mechanics.Application.Notification.Services;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mechanics.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notification")]
[TestCategory("Email")]
public class EmailServiceTests
{
    [TestMethod]
    public async Task It_ShouldSendEmail_WhenWorkOrderIsCreated()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };

        // Act
        await service.SendWorkOrderCreated(customer, workOrder, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains($"{workOrder.AccessKey[..4]} {workOrder.AccessKey[4..]}", emailMessage.Body);
    }

    [TestMethod]
    public async Task It_ShouldSendEmail_WhenWorkOrderStatusChanges()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
            Status = WorkOrderStatus.InProgress,
        };

        // Act
        await service.SendWorkOrderStatusChanged(customer, workOrder, WorkOrderStatus.PendingApproval, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains(workOrder.AccessKey, emailMessage.Body);
    }

    [TestMethod]
    public async Task It_ShouldSendEmail_WhenWorkOrderIsCancelled()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };

        // Act
        await service.SendWorkOrderCancelled(customer, workOrder, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains("foi cancelada", emailMessage.Body);
    }

    [TestMethod]
    public async Task It_ShouldSendEmail_WhenWorkOrderDeliveredSurveyIsRequested()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };

        // Act
        await service.SendWorkOrderDeliveredSurvey(customer, workOrder, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains("foi entregue", emailMessage.Body);
    }

    private static EmailService CreateInstance(IEmailSenderService senderService) =>
        new(new NullLoggerFactory().CreateLogger<EmailService>(), senderService);
}
