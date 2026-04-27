using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.WorkOrders;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mechanics.Tests.Unit.Tests.WorkOrders;

[TestClass]
[TestCategory("WorkOrder")]
[TestCategory("Budget")]
public class BudgetAppServiceTests
{
    public TestContext TestContext { get; set; }

    private readonly EmailServiceMock _emailMock = new();
    private readonly NullLoggerFactory _loggerFactory = new();

    #region criar orçamento

    [TestMethod("deve criar orçamento quando os dados forem válidos")]
    public async Task It_ShouldCreateBudget_WhenDataIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();

        var service = ServicesCatalogMocks.CreateService(Guid.NewGuid(), name: "Diagnóstico");
        var wo = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), customerId, vehicleId, service);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(CustomerMocks.CreateCustomerPf(customerId));
                ctx.ServiceCatalog.Add(service);
                ctx.WorkOrders.Add(wo);
                wo.ServiceCatalog = [service];
                wo.Products = [ProductMocks.CreateWorkOrderProduct()];
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act
        await handler.CreateAndSendBudget(wo.Id, performedBy, TestContext.CancellationTokenSource.Token);

        // Assert
        var budget = await context.Budgets.AsNoTracking().Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.WorkOrderId == wo.Id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(budget, "Budget deve ser persistido");
        Assert.IsGreaterThan(0, budget.Items!.Count, "Itens devem ser criados");
        Assert.AreEqual(BudgetStatus.Sent, budget.Status, "Status deve ser Sent");
        Assert.IsNotNull(budget.ExpiresAt, "ExpiresAt deve ser definido");
        Assert.IsGreaterThan(0, budget.Total, "Total deve ser calculado");

        var reloadedWo = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(reloadedWo);
        Assert.AreEqual(WorkOrderStatus.PendingApproval, reloadedWo.Status, "WO deve ficar PendingApproval");

        // histórico
        var history = await context.WorkOrderHistories.AsNoTracking()
            .Where(h => h.WorkOrderId == wo.Id && h.Action == "BudgetSent")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.HasCount(1, history, "Histórico deve ser gerado");
    }

    [TestMethod("deve lançar exceção quando não tiver itens")]
    public async Task It_ShouldThrow_WhenNoItemsOnWorkOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.WorkOrders.Add(wo))
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act + Assert
        await Assert.ThrowsExactlyAsync<BusinessException>(async () =>
        {
            await handler.CreateAndSendBudget(wo.Id, performedBy, TestContext.CancellationTokenSource.Token);
        });
    }

    [TestMethod("deve gerar histórico em caso de sucesso")]
    public async Task It_ShouldGenerateHistory_OnCreateSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();
        var service = ServicesCatalogMocks.CreateService(Guid.NewGuid(), name: "Alinhamento");
        var wo = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), customerId, vehicleId, service);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.ServiceCatalog.Add(service);
                ctx.WorkOrders.Add(wo);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act
        await handler.CreateAndSendBudget(wo.Id, performedBy, TestContext.CancellationTokenSource.Token);

        // Assert
        var hist = await context.WorkOrderHistories.AsNoTracking()
            .Where(h => h.WorkOrderId == wo.Id && h.Action == "BudgetSent")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.HasCount(1, hist, "Deve registrar 1 histórico de envio de orçamento");
    }

    #endregion

    #region aprovar orçamento

    [TestMethod("deve aprovar o orçamento quando os dados forem válidos")]
    public async Task It_ShouldApproveBudget_WhenDataIsValid()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);

        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent,
            CreationDate = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(2),
            Items = new List<BudgetItem>
            {
                new()
                {
                    BudgetId = Guid.NewGuid(), NameSnapshot = "Serviço", Quantity = 1, UnitPriceSnapshot = 100,
                    Subtotal = 100,
                },
            },
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act
        await handler.ApproveBudget(customer.Id, wo.AccessKey, "OK", TestContext.CancellationTokenSource.Token);

        // Assert
        var updated = await context.Budgets.AsNoTracking()
            .FirstOrDefaultAsync(b => b.WorkOrderId == wo.Id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(BudgetStatus.Approved, updated.Status);
        Assert.IsNotNull(updated.ApprovedAt);

        var hist = await context.WorkOrderHistories.AsNoTracking()
            .Where(h => h.WorkOrderId == wo.Id && h.Action == "BudgetApprovedPublic")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.HasCount(1, hist, "Deve registrar histórico de aprovação pública");
    }

    [TestMethod("deve lançar exceção se o orçamento já tiver sido aprovado")]
    public async Task It_ShouldThrow_WhenBudgetAlreadyApproved()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent, // filtrado por Sent
            CreationDate = DateTime.Now,
            ApprovedAt = DateTime.Now.AddMinutes(-5), // já aprovado
            Items = new List<BudgetItem>(),
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act + Assert
        await Assert.ThrowsExactlyAsync<BusinessException>(async () =>
        {
            await handler.ApproveBudget(customer.Id, wo.AccessKey, null,
                TestContext.CancellationTokenSource.Token);
        });
    }

    [TestMethod("deve lançar exceção quando o orçamento estiver expirado ao tentar aprovar")]
    public async Task It_ShouldThrow_WhenApprovingExpiredBudget()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent,
            CreationDate = DateTime.Now.AddDays(-5),
            ExpiresAt = DateTime.Now.AddMinutes(-1), // expirado
            Items = new List<BudgetItem>
            {
                new()
                {
                    BudgetId = Guid.NewGuid(), NameSnapshot = "Serviço", Quantity = 1, UnitPriceSnapshot = 50,
                    Subtotal = 50,
                },
            },
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act + Assert
        await Assert.ThrowsExactlyAsync<BusinessException>(async () =>
        {
            await handler.ApproveBudget(customer.Id, wo.AccessKey, "trying after expired",
                TestContext.CancellationTokenSource.Token);
        });

        // Verifica que status foi marcado como Expired
        var updated = await context.Budgets.AsNoTracking()
            .FirstOrDefaultAsync(b => b.WorkOrderId == wo.Id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(BudgetStatus.Expired, updated.Status);
        Assert.IsNull(updated.ApprovedAt, "Não deve aprovar orçamento expirado");
    }

    #endregion

    #region rejeitar orçamento

    [TestMethod("deve rejeitar o orçamento se os dados forem válidos")]
    public async Task It_ShouldRejectBudget_WhenDataIsValid()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent,
            CreationDate = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(1),
            Items = new List<BudgetItem>(),
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act
        await handler.RejectBudget(customer.Id, wo.AccessKey, "muito caro",
            TestContext.CancellationTokenSource.Token);

        // Assert
        var updated = await context.Budgets.AsNoTracking()
            .FirstOrDefaultAsync(b => b.WorkOrderId == wo.Id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(BudgetStatus.Rejected, updated.Status);
        Assert.IsNotNull(updated.RejectedAt);

        var reloadedWo = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(reloadedWo);
        Assert.AreEqual(WorkOrderStatus.UnderDiagnosis, reloadedWo.Status);
    }

    [TestMethod("deve gerar histórico quando o orçamento for rejeitado")]
    public async Task It_ShouldGenerateHistory_OnReject()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent,
            CreationDate = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(1),
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act
        await handler.RejectBudget(customer.Id, wo.AccessKey, null, TestContext.CancellationTokenSource.Token);

        // Assert
        var histories = await context.WorkOrderHistories.AsNoTracking()
            .Where(h => h.WorkOrderId == wo.Id && h.Action == "BudgetRejectedByCustomer")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.HasCount(1, histories, "Deve registrar histórico de rejeição");
    }

    [TestMethod("deve lançar exceção quando o orçamento estiver expirado ao tentar rejeitar")]
    public async Task It_ShouldThrow_WhenRejectingExpiredBudget()
    {
        // Arrange
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var vehicleId = Guid.NewGuid();
        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer.Id, vehicleId);
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            Status = BudgetStatus.Sent,
            CreationDate = DateTime.Now.AddDays(-4),
            ExpiresAt = DateTime.Now.AddMinutes(-1), // expirado
            Items = new List<BudgetItem>
            {
                new()
                {
                    BudgetId = Guid.NewGuid(), NameSnapshot = "Serviço", Quantity = 1, UnitPriceSnapshot = 80,
                    Subtotal = 80,
                },
            },
        };

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(customer);
                ctx.WorkOrders.Add(wo);
                ctx.Budgets.Add(budget);
            })
            .Build();

        var handler = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());

        // Act + Assert
        await Assert.ThrowsExactlyAsync<BusinessException>(async () =>
        {
            await handler.RejectBudget(customer.Id, wo.AccessKey, "reject after expired",
                TestContext.CancellationTokenSource.Token);
        });

        // Verifica que status foi marcado como Expired
        var updated = await context.Budgets.AsNoTracking()
            .FirstOrDefaultAsync(b => b.WorkOrderId == wo.Id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(BudgetStatus.Expired, updated.Status);
        Assert.IsNull(updated.RejectedAt, "Não deve rejeitar orçamento expirado");
    }

    #endregion
}
