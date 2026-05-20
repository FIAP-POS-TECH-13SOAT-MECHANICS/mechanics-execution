using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Budgets.Services;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.WorkOrders.Events;
using Mechanics.Domain.Budgets;
using Mechanics.Domain.WorkOrders;
using Mechanics.Application.Identity.Responses;
using Mechanics.Infra.Data;
using Mechanics.Infra.Messaging.Publishers;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanics.Tests.Unit.Tests.Budgets;

[TestClass]
[TestCategory("Budget")]
public class BudgetAppServiceTests
{
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private IdentityApiServiceMock _identityApiServiceMock = null!;
    private ILoggerFactory _loggerFactory = null!;
    private BudgetAppService _service = null!;
    private AppDbContext _context = null!;
    private BudgetRevisedEvent _message = null!;

    [TestInitialize]
    public void Initialize()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _identityApiServiceMock = new IdentityApiServiceMock();
        _loggerFactory = new NullLoggerFactory();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    [TestMethod("UpdateBudget should set WorkOrder status to InProgress when approved")]
    public async Task UpdateBudget_WhenApproved_ShouldSetWorkOrderStatusToInProgress()
    {
        // Arrange
        SetupScenario(approved: true);

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        var updatedWo = await _context.WorkOrders.FindAsync(_message.WorkOrderId);
        AreEqual(WorkOrderStatus.InProgress, updatedWo!.Status);
    }

    [TestMethod("UpdateBudget should set Budget status to Approved when approved")]
    public async Task UpdateBudget_WhenApproved_ShouldSetBudgetStatusToApproved()
    {
        // Arrange
        SetupScenario(approved: true);

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        var updatedBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.WorkOrderId == _message.WorkOrderId);
        AreEqual(BudgetStatus.Approved, updatedBudget!.Status);
    }

    [TestMethod("UpdateBudget should publish status changed event when approved")]
    public async Task UpdateBudget_WhenApproved_ShouldPublishEvent()
    {
        // Arrange
        SetupScenario(approved: true);

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(x => x.PublishAsync(It.Is<WorkOrderStatusChangedEvent>(e =>
            e.WorkOrderId == _message.WorkOrderId &&
            e.NewStatus == nameof(WorkOrderStatus.InProgress)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod("UpdateBudget should send email notification when approved")]
    public async Task UpdateBudget_WhenApproved_ShouldSendNotification()
    {
        // Arrange
        SetupScenario(approved: true);

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendCustomerResponse(It.IsAny<UserResponse>(), It.IsAny<WorkOrder>(), It.IsAny<Budget>(), _message,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod("UpdateBudget should do nothing when budget is already approved")]
    public async Task UpdateBudget_WhenAlreadyApproved_ShouldDoNothing()
    {
        // Arrange
        // O código do serviço busca por orçamentos com status 'Sent'.
        // Se message.Approved for true e budget.ApprovedAt != null, ele ignora.
        SetupScenario(approved: true, budgetStatus: BudgetStatus.Sent);

        var budget = await _context.Budgets.FirstAsync();
        budget.ApprovedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<WorkOrderStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendCustomerResponse(It.IsAny<UserResponse>(), It.IsAny<WorkOrder>(), It.IsAny<Budget>(), _message, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod("UpdateBudget should set status to Expired when rejected and past expiration date")]
    public async Task UpdateBudget_WhenRejectedAndExpired_ShouldSetStatusToExpired()
    {
        // Arrange
        SetupScenario(approved: false, creationDate: DateTime.Now.AddDays(-5));

        // Act
        await _service.UpdateBudget(_message, CancellationToken.None);

        // Assert
        var updatedBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.WorkOrderId == _message.WorkOrderId);
        AreEqual(BudgetStatus.Expired, updatedBudget!.Status);

        _emailServiceMock.Verify(x => x.SendExpiredBudget(It.IsAny<UserResponse>(), It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupScenario(
        bool approved = true,
        WorkOrderStatus woStatus = WorkOrderStatus.PendingApproval,
        BudgetStatus budgetStatus = BudgetStatus.Sent,
        DateTime? creationDate = null)
    {
        var workOrderId = Guid.NewGuid();
        var mechanicId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var effectiveCreationDate = creationDate ?? DateTime.Now.AddDays(-1);

        var wo = WorkOrderMocks.CreateWorkOrderEntity(workOrderId, Guid.NewGuid(), Guid.NewGuid(), mechanicId);
        wo.Status = woStatus;
        wo.LastUpdate = effectiveCreationDate;

        var budget = BudgetMocks.CreateBudget(budgetId, workOrderId, budgetStatus, effectiveCreationDate);

        _identityApiServiceMock.UserToReturn = new UserResponse
        {
            Id = mechanicId,
            FullName = "Mechanic",
            Email = "mechanic@test.com",
            CpfNumber = "12345678901",
            Role = new RoleResponse { Id = Guid.NewGuid(), Name = "Mechanic" }
        };

        _context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        _service = new BudgetAppService(
            _context,
            _emailServiceMock.Object,
            _eventPublisherMock.Object,
            _identityApiServiceMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        _message = new BudgetRevisedEvent
        {
            WorkOrderId = workOrderId,
            Approved = approved,
            Notes = approved ? "Approved by customer" : "Rejected",
            OccurredAt = DateTimeOffset.Now
        };
    }
}
