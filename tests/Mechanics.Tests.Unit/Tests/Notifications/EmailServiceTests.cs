using Mechanics.Application.Notification.Services;
using Mechanics.Domain.Auth;
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
    public async Task It_ShouldSendEmail_WhenWorkOrderIsPendingApproval()
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

        var budget = new Budget
        {
            WorkOrderId = workOrder.Id,
            Status = BudgetStatus.Sent,
            Total = 123.45m,
            Items = [],
        };

        // Act
        await service.SendWorkOrderPendingApproval(customer, workOrder, budget, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains("Resumo dos itens", emailMessage.Body);
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
    public async Task It_ShouldSendEmail_WhenMechanicBudgetIsApproved()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var mechanic = UserMocks.CreateUser(Guid.NewGuid(), "Joao Mecânico", "70629831017", RoleNames.Mechanic);

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };

        var budget = new Budget
        {
            WorkOrderId = workOrder.Id,
            Status = BudgetStatus.Sent,
            Total = 987.65m,
            Items = [],
        };

        // Act
        await service.SendMechanicBudgetDecision(mechanic, workOrder, budget, approved: true, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(mechanic.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains("aprovado", emailMessage.Subject);
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

    [TestMethod]
    public async Task It_ShouldSendEmail_WithUserPasswordCreationCode()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var mechanic = UserMocks.CreateUser("maria.dev", "123456");
        const string code = "ABCD-1234";

        // Act
        await service.SendUserPasswordCreationCode(mechanic, code, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(mechanic.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains(code, emailMessage.Body);
    }

    [TestMethod]
    public async Task It_ShouldSendEmail_WithCustomerUserPasswordCreationCode()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var user = UserMocks.CreateUser("cliente.teste", "12345678901");
        const string code = "YHLur6lSn";

        // Act
        await service.SendCustomerUserPasswordCreationCode(user, code, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(user.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains(code, emailMessage.Body);
        Assert.Contains(user.CpfNumber, emailMessage.Body);
        Assert.Contains("ordens de serviço", emailMessage.Body);
    }

    [TestMethod]
    public async Task It_ShouldSendEmail_WhenUserPasswordIsChanged()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var user = UserMocks.CreateUser("carlos.teste", "123456");

        // Act
        await service.UserPasswordChanged(user, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(user.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.IsTrue(emailMessage.Body.Contains("senha", StringComparison.OrdinalIgnoreCase));
    }

    private static EmailService CreateInstance(IEmailSenderService senderService) =>
        new(new NullLoggerFactory().CreateLogger<EmailService>(), senderService);
}
