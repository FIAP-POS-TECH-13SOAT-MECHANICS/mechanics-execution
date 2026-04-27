using AutoMapper;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Mechanics.Tests.Unit.Tests.WorkOrders;

[TestClass]
[TestCategory("WorkOrder")]
public class WorkOrderAppServiceTests
{
    public TestContext TestContext { get; set; }

    private IMapper _mapper = null!;
    private EmailServiceMock _emailMock = null!;
    private NullLoggerFactory _loggerFactory = null!;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = AutoMapperFactory.CreateMap("WorkOrders");

        _emailMock = new EmailServiceMock();
        _loggerFactory = new NullLoggerFactory();
    }

    #region Criar OSs

    [TestMethod("Create should persist work order and send created email")]
    public async Task Create_ShouldPersistAndSendEmail()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "John", Email = "john@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12345678909"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = VehicleColor.White, Year = "2020",
                    LicensePlate = new LicensePlate("ABC1234"), Chassis = "CH", OwnerId = customerId,
                });
                ctx.Products.Add(new Product
                {
                    Id = productId,
                    Name = "Óleo de Motor",
                    Description = "Óleo sintético 5W30",
                    Quantity = 1,
                    Status = ProductStatusType.Active,
                    Type = ProductType.Part,
                });
                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = serviceId,
                    Name = "Troca de Óleo",
                    Description = "Troca completa de óleo",
                    BasePrice = 80m,
                    AverageTime = 40,
                    Status = ServiceCatalogStatusType.Active,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new CreateWorkOrderRequest
        {
            VehicleId = vehicleId,
            Products = [new WorkOrderProductRequest { ProductId = productId, Quantity = 1 }],
            ServiceCatalogIds = [serviceId],
            ReportedProblem = "Test problem",
        };

        var id = await service.Create(request, TestContext.CancellationTokenSource.Token);

        var wo = await context.WorkOrders.FindAsync([id.CreatedId], TestContext.CancellationTokenSource.Token);
        IsNotNull(wo, "Work order should be persisted");
        AreEqual(customerId, wo.CustomerId, "CustomerId persisted");
        IsTrue(_emailMock.SendWorkOrderCreatedCalled, "SendWorkOrderCreated should be called");
    }

    [TestMethod("Create should throw when vehicle owner customer does not exist")]
    public async Task Create_ShouldThrow_WhenVehicleOwnerCustomerMissing()
    {
        var orphanOwnerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2022",
                    LicensePlate = new LicensePlate("ORP1234"),
                    Chassis = "CHORP",
                    OwnerId = orphanOwnerId,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new CreateWorkOrderRequest
        {
            VehicleId = vehicleId,
        };

        await ThrowsExactlyAsync<EntityNotFoundException>(() =>
            service.Create(request, TestContext.CancellationTokenSource.Token));
    }

    #endregion

    #region Alterar status

    [TestMethod("RequestApproval should calculate estimate, set PendingApproval and send email")]
    public async Task RequestApproval_ShouldCalculateEstimateAndSendEmail()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var svcId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "Mary", Email = "mary@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "98765432100"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = VehicleColor.Black, Year = "2021",
                    LicensePlate = new LicensePlate("DEF5678"), Chassis = "CH2", OwnerId = customerId,
                });
                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svcId, Name = "Oil change", Description = "Change oil", BasePrice = 100m, AverageTime = 30,
                    Status = ServiceCatalogStatusType.Active,
                });
            })
            .Build();

        var svc = await context.ServiceCatalog.FindAsync([svcId], TestContext.CancellationTokenSource.Token);
        var wo = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), customerId, vehicleId, svc!);

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var performedBy = Guid.NewGuid();
        await service.RequestApproval(wo.Id, performedBy, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.PendingApproval, reloaded.Status);
        IsNotNull(reloaded.ApprovalRequestedAt);
        AreEqual(performedBy, reloaded.LastStatusChangeBy);
        IsTrue(_emailMock.SendWorkOrderPendingApprovalCalled);

        AreEqual(100m, _emailMock.LastBudgetTotal);
    }

    [TestMethod("ChangeStatus should require approval before InProgress and should record history")]
    public async Task ChangeStatus_ShouldValidateAndRecordHistory()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Pedro",
                    Email = "pedro@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "11122233344"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.Gray,
                    Year = "2019",
                    LicensePlate = new LicensePlate("GHI9012"),
                    Chassis = "CH3",
                    OwnerId = customerId,
                });
            })
            .Build();

        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);
        wo.Status = WorkOrderStatus.PendingApproval;
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var mechanicRoleId = Guid.NewGuid();
        var mechanicRole = new Role
        {
            Id = mechanicRoleId,
            Name = RoleNames.Mechanic,
            CreationDate = DateTime.UtcNow,
        };
        context.Roles.Add(mechanicRole);

        var performingUserId = Guid.NewGuid();
        var performingUser = new User
        {
            Id = performingUserId,
            FullName = "Test Mechanic",
            CpfNumber = "45678901234",
            Email = "test_mechanic@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = mechanicRoleId,
            CreationDate = DateTime.UtcNow,
        };
        context.Users.Add(performingUser);

        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<BusinessException>(() =>
            service.ChangeStatus(wo.Id, WorkOrderStatus.Received, performingUserId, comment: null,
                TestContext.CancellationTokenSource.Token));

        wo.ApprovedAt = DateTime.Now;
        context.WorkOrders.Update(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var statusChangedBy = Guid.NewGuid();
        var otherUser = new User
        {
            Id = statusChangedBy,
            FullName = "Another Mechanic",
            CpfNumber = "56789012345",
            Email = "another_mechanic@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = mechanicRoleId,
            CreationDate = DateTime.UtcNow,
        };
        context.Users.Add(otherUser);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await service.ChangeStatus(wo.Id, WorkOrderStatus.InProgress, statusChangedBy, comment: null,
            TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.InProgress, reloaded.Status);
        AreEqual(statusChangedBy, reloaded.LastStatusChangeBy);
        var histories = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id && h.Action == "StatusChanged")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        IsNotEmpty(histories);
        IsTrue(_emailMock.SendWorkOrderStatusChangedCalled);
    }

    [TestMethod("ChangeStatus should reject same status and skip notifications")]
    public async Task ChangeStatus_ShouldRejectSameStatus()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Laura",
                    Email = "laura@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "55566677788"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.Red,
                    Year = "2018",
                    LicensePlate = new LicensePlate("JKL3456"),
                    Chassis = "CH4",
                    OwnerId = customerId,
                });
            })
            .Build();

        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);
        AreEqual(WorkOrderStatus.Received, wo.Status);

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = RoleNames.Mechanic,
            CreationDate = DateTime.UtcNow,
        };
        context.Roles.Add(role);

        var actorId = Guid.NewGuid();
        var actor = new User
        {
            Id = actorId,
            FullName = "Actor User",
            CpfNumber = "67890123456",
            Email = "actor@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = roleId,
            CreationDate = DateTime.UtcNow,
        };
        context.Users.Add(actor);

        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<BusinessException>(() =>
            service.ChangeStatus(wo.Id, WorkOrderStatus.Received, actorId, comment: null,
                TestContext.CancellationTokenSource.Token));

        IsFalse(_emailMock.SendWorkOrderStatusChangedCalled, "Status change email should not be sent");
        var histories = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id)
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        IsEmpty(histories, "No history should be recorded");

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.Received, reloaded.Status);
    }

    [TestMethod("IsTransitionAllowed deve permitir o fluxo principal e rejeitar transições inválidas ou iguais")]
    public void IsTransitionAllowed_ValidAndInvalidTransitions()
    {
        IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.UnderDiagnosis));
        IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.UnderDiagnosis, WorkOrderStatus.PendingApproval));
        IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.PendingApproval, WorkOrderStatus.InProgress));
        IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.InProgress, WorkOrderStatus.Completed));
        IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.Completed, WorkOrderStatus.Delivered));

        // transição inválida (pular etapas)
        IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.InProgress));

        // transição para o mesmo status deve ser considerada inválida no método
        IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.Received));
        IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Completed, WorkOrderStatus.Completed));
    }

    private static bool InvokeIsTransitionAllowed(WorkOrderStatus from, WorkOrderStatus to)
    {
        var method = typeof(WorkOrderAppService).GetMethod("IsTransitionAllowed", BindingFlags.NonPublic | BindingFlags.Static);
        IsNotNull(method, "Método IsTransitionAllowed não encontrado. Verifique a assinatura e a visibilidade.");
        return (bool)method.Invoke(null, [from, to])!;
    }

    #endregion

    #region Alterar detalhes

    [TestMethod("UpdateDetails should add products, services and observations and record history")]
    public async Task UpdateDetails_ShouldAddProductsServicesAndObservations()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var mechanicUserId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Client",
                    Email = "client@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12345678909"),
                });

                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("ABC1234"),
                    Chassis = "CH",
                    OwnerId = customerId,
                });

                ctx.Products.Add(new Product
                {
                    Id = productId,
                    Name = "Filtro",
                    Description = "Filtro de óleo",
                    Quantity = 5,
                    Status = ProductStatusType.Active,
                    Type = ProductType.Part,
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = serviceId,
                    Name = "Troca de Filtro",
                    Description = "Troca de filtro",
                    BasePrice = 50m,
                    AverageTime = 20,
                    Status = ServiceCatalogStatusType.Active,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        // create base work order without products/services
        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var req = new UpdateWorkOrderRequest
        {
            Products = [new WorkOrderProductRequest { ProductId = productId, Quantity = 1 }],
            ServiceIds = [serviceId],
            Observations = "Substituir filtro e testar motor",
        };

        await service.UpdateDetails(wo.Id, req, mechanicUserId, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == wo.Id, TestContext.CancellationTokenSource.Token);

        IsNotNull(reloaded);
        IsTrue(reloaded.Products != null && reloaded.Products.Any(p => p.ProductId == productId));
        IsTrue(reloaded.ServiceCatalog != null && reloaded.ServiceCatalog.Any(s => s.Id == serviceId));
        AreEqual(req.Observations, reloaded.Observations);

        var history = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id && h.Action == "DetailsUpdated")
            .FirstOrDefaultAsync(TestContext.CancellationTokenSource.Token);
        IsNotNull(history);
        IsNotNull(history.Details);
        AreEqual(mechanicUserId, history.PerformedByUserId);
        Contains("AddedProducts:1", history.Details);
        Contains("AddedServices:1", history.Details);
        Contains("ObservationsUpdated", history.Details);
    }

    [TestMethod("Assign should set mechanic, record history and auto-transition from Received")]
    public async Task Assign_ShouldAssignMechanic_RecordHistory_AndAutoTransition()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        var assignedToUserId = Guid.NewGuid();
        var performedByUserId = Guid.NewGuid();
        var mechanicRoleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                // base customer/vehicle
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Client",
                    Email = "client@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12345678909"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("AAA1B23"),
                    Chassis = "CH",
                    OwnerId = customerId,
                });

                // mechanic role
                ctx.Roles.Add(new Role { Id = mechanicRoleId, Name = RoleNames.Mechanic, CreationDate = DateTime.UtcNow });

                // assigned user (mechanic)
                ctx.Users.Add(new User
                {
                    Id = assignedToUserId,
                    FullName = "Assigned Mechanic",
                    CpfNumber = "78901234567",
                    Email = "assigned@example.com",
                    PasswordHash = "hash",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = mechanicRoleId,
                    CreationDate = DateTime.UtcNow,
                });

                // performing user (could also be mechanic)
                ctx.Users.Add(new User
                {
                    Id = performedByUserId,
                    FullName = "Supervisor",
                    CpfNumber = "89012345678",
                    Email = "sup@example.com",
                    PasswordHash = "hash",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = mechanicRoleId,
                    CreationDate = DateTime.UtcNow,
                });

                // work order in Received
                ctx.WorkOrders.Add(new WorkOrder
                {
                    Id = workOrderId,
                    CustomerId = customerId,
                    VehicleId = vehicleId,
                    AccessKey = WorkOrder.GenerateNewAccessKey([]),
                    Status = WorkOrderStatus.Received,
                    CreationDate = DateTime.Now,
                    LastUpdate = DateTime.Now,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        const string comment = "Take this ASAP";
        await service.Assign(workOrderId, assignedToUserId, performedByUserId, comment, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync([workOrderId], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(assignedToUserId, reloaded.AssignedToUserId, "AssignedToUserId updated");

        // assignment history
        var histAssign = await context.WorkOrderHistories
            .Where(h => h.WorkOrderId == workOrderId && h.Action == "Assigned")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        HasCount(1, histAssign, "One assignment history should be recorded");
        Contains(assignedToUserId.ToString(), histAssign[0].Details!);
        Contains(comment, histAssign[0].Details!);
        AreEqual(performedByUserId, histAssign[0].PerformedByUserId);

        // Auto-transition from Received -> UnderDiagnosis should have occurred and notified
        AreEqual(WorkOrderStatus.UnderDiagnosis, reloaded.Status, "Auto-transition to UnderDiagnosis expected");
        IsTrue(_emailMock.SendWorkOrderStatusChangedCalled, "Status changed email should be sent due to auto-transition");

        var histStatus = await context.WorkOrderHistories
            .Where(h => h.WorkOrderId == workOrderId && h.Action == "StatusChanged")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        IsNotEmpty(histStatus, "Status change history should be recorded");
    }

    [TestMethod("Assign should throw when assigned user is not a mechanic")]
    public async Task Assign_ShouldThrow_WhenAssignedUserIsNotMechanic()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        var assignedToUserId = Guid.NewGuid();
        var nonMechanicRoleId = Guid.NewGuid();
        var performerId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "C", Email = "c@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "11122233344"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "M", Model = "X", Color = VehicleColor.Black, Year = "2022",
                    LicensePlate = new LicensePlate("BBB1C23"), Chassis = "CH2", OwnerId = customerId,
                });

                // non-mechanic role
                ctx.Roles.Add(new Role { Id = nonMechanicRoleId, Name = "Admin", CreationDate = DateTime.UtcNow });

                // non-mechanic user
                ctx.Users.Add(new User
                {
                    Id = assignedToUserId, FullName = "Admin User", CpfNumber = "11122233344", Email = "admin@example.com",
                    PasswordHash = "h", SecurityStamp = Guid.NewGuid().ToString(), RoleId = nonMechanicRoleId,
                    CreationDate = DateTime.UtcNow,
                });
                ctx.Users.Add(new User
                {
                    Id = performerId, FullName = "Perf", CpfNumber = "22233344455", Email = "perf@example.com", PasswordHash = "h",
                    SecurityStamp = Guid.NewGuid().ToString(), RoleId = nonMechanicRoleId, CreationDate = DateTime.UtcNow,
                });

                ctx.WorkOrders.Add(new WorkOrder
                {
                    Id = workOrderId, CustomerId = customerId, VehicleId = vehicleId,
                    AccessKey = WorkOrder.GenerateNewAccessKey([]), Status = WorkOrderStatus.Received, CreationDate = DateTime.Now,
                    LastUpdate = DateTime.Now,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        await ThrowsExactlyAsync<BusinessException>(() =>
            service.Assign(workOrderId, assignedToUserId, performerId, null, TestContext.CancellationTokenSource.Token));
    }

    [TestMethod("Assign should throw EntityNotFound when WorkOrder or User does not exist")]
    public async Task Assign_ShouldThrow_WhenWorkOrderOrUserNotFound()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var mechanicRoleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "C", Email = "c@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "99988877766"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "M", Model = "Y", Color = VehicleColor.Gray, Year = "2020",
                    LicensePlate = new LicensePlate("CCC1D23"), Chassis = "CH3", OwnerId = customerId,
                });
                ctx.Roles.Add(new Role { Id = mechanicRoleId, Name = RoleNames.Mechanic, CreationDate = DateTime.UtcNow });
            })
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        // 1) WorkOrder not found
        var missingWoId = Guid.NewGuid();
        var someUserId = Guid.NewGuid();
        await context.Users.AddAsync(new User
        {
            Id = someUserId, FullName = "Mec A", CpfNumber = "33344455566", Email = "meca@example.com", PasswordHash = "h",
            SecurityStamp = Guid.NewGuid().ToString(), RoleId = mechanicRoleId, CreationDate = DateTime.UtcNow,
        }, TestContext.CancellationTokenSource.Token);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<EntityNotFoundException>(() =>
            service.Assign(missingWoId, someUserId, someUserId, null, TestContext.CancellationTokenSource.Token));

        // 2) User not found
        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(), CustomerId = customerId, VehicleId = vehicleId, AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received, CreationDate = DateTime.Now, LastUpdate = DateTime.Now,
        };
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<EntityNotFoundException>(() =>
            service.Assign(wo.Id, Guid.NewGuid(), someUserId, null, TestContext.CancellationTokenSource.Token));
    }

    #endregion

    #region Consultar por ID ou documento

    [TestMethod("Get should return mapped response with ProductIds and ServiceCatalogIds")]
    public async Task Get_ShouldReturnMappedResponse_WithProductsAndServices()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "C", Email = "c@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "22233344455"),
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "M", Model = "Z", Color = VehicleColor.Blue, Year = "2019",
                    LicensePlate = new LicensePlate("DDD1E23"), Chassis = "CH4", OwnerId = customerId,
                });
                ctx.Products.Add(new Product
                {
                    Id = productId, Name = "P1", Description = "D1", Quantity = 10, Status = ProductStatusType.Active,
                    Type = ProductType.Part,
                });
                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = serviceId, Name = "S1", Description = "SD1", BasePrice = 10m, AverageTime = 10,
                    Status = ServiceCatalogStatusType.Active,
                });
            })
            .Build();

        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
        };

        // attach relations
        var prod = await context.Products.FindAsync([productId], TestContext.CancellationTokenSource.Token);
        var svc = await context.ServiceCatalog.FindAsync([serviceId], TestContext.CancellationTokenSource.Token);
        wo.Products = [new WorkOrderProduct { Product = prod, Quantity = 1 }];
        wo.ServiceCatalog = new List<ServiceCatalog> { svc! };

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var resp = await service.Get(wo.Id, TestContext.CancellationTokenSource.Token);
        IsNotNull(resp, "Response should not be null");
        AreEqual(wo.Id, resp.Id);
        AreEqual(wo.AccessKey, resp.AccessKey);
        AreEqual(wo.Status, resp.Status);
        AreEqual(wo.CustomerId, resp.CustomerId);
        AreEqual(wo.VehicleId, resp.VehicleId);
        IsTrue(resp.Products != null && resp.Products.Select(p => p.ProductId).Contains(productId));
        IsTrue(resp.ServiceCatalogIds != null && resp.ServiceCatalogIds.Contains(serviceId));
    }

    [TestMethod("Get should return null when WorkOrder not found")]
    public async Task Get_ShouldReturnNull_WhenNotFound()
    {
        await using var context = new DbContextTestBuilder().Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var resp = await service.Get(Guid.NewGuid(), TestContext.CancellationTokenSource.Token);
        IsNull(resp);
    }

    [TestMethod("TrackByAccessKey should return WorkOrder for matching customer id and access key")]
    public async Task TrackByAccessKey_ShouldReturnWorkOrder_WhenCustomerAndAccessKeyMatch()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        const string rawDocument = "12345678909";
        const string storedAccessKey = "123123";
        const string queryAccessKey = "1 2 3 1 2 3";

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Jane",
                    Email = "jane@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, rawDocument),
                });

                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("ABC1D23"),
                    Chassis = "CHX",
                    OwnerId = customerId,
                });

                ctx.WorkOrders.Add(new WorkOrder
                {
                    Id = workOrderId,
                    CustomerId = customerId,
                    VehicleId = vehicleId,
                    AccessKey = storedAccessKey,
                    Status = WorkOrderStatus.Received,
                    CreationDate = DateTime.Now,
                    LastUpdate = DateTime.Now,
                });
            })
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var resp = await service.TrackByAccessKey(customerId, queryAccessKey,
            TestContext.CancellationTokenSource.Token);

        IsNotNull(resp, "Response should not be null");
        AreEqual(workOrderId, resp.Id, "Returned work order id should match");
        AreEqual(storedAccessKey.Replace(" ", string.Empty), resp.AccessKey, "AccessKey should match stored value");
        AreEqual(customerId, resp.CustomerId, "CustomerId should match");
        AreEqual(vehicleId, resp.VehicleId, "VehicleId should match");
    }

    #endregion

    #region Consultar tempo médio

    [TestMethod("GetAverageServiceTime should return total average time for associated services")]
    public async Task GetAverageServiceTime_ShouldReturnSumOfAverageTimes()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var svc1Id = Guid.NewGuid();
        var svc2Id = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Client",
                    Email = "client@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12312312312"),
                });

                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("AVG1234"),
                    Chassis = "CHAVG",
                    OwnerId = customerId,
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svc1Id,
                    Name = "Service 1",
                    Description = "S1",
                    BasePrice = 10m,
                    AverageTime = 30,
                    Status = ServiceCatalogStatusType.Active,
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svc2Id,
                    Name = "Service 2",
                    Description = "S2",
                    BasePrice = 20m,
                    AverageTime = 45,
                    Status = ServiceCatalogStatusType.Active,
                });
            })
            .Build();

        var svc1 = await context.ServiceCatalog.FindAsync([svc1Id], TestContext.CancellationTokenSource.Token);
        var svc2 = await context.ServiceCatalog.FindAsync([svc2Id], TestContext.CancellationTokenSource.Token);

        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
            ServiceCatalog = new List<ServiceCatalog> { svc1!, svc2! },
        };

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var response = await service.GetAverageServiceTime(wo.Id, TestContext.CancellationTokenSource.Token);

        IsNotNull(response);
        AreEqual(wo.Id, response.WorkOrderId);
        AreEqual(30 + 45, response.TotalAverageTime);
    }

    [TestMethod("GetAverageServiceTime should throw when work order not found")]
    public async Task GetAverageServiceTime_ShouldThrowWhenWorkOrderNotFound()
    {
        await using var context = new DbContextTestBuilder().Build();
        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        await ThrowsAsync<EntityNotFoundException>(async () =>
            await service.GetAverageServiceTime(Guid.NewGuid(), TestContext.CancellationTokenSource.Token));
    }

    #endregion

    #region Listar OSs

    [TestMethod("GetList should filter by CustomerId")]
    public async Task GetList_ShouldFilterByCustomerId()
    {
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();
        var vehicleA1 = Guid.NewGuid();
        var vehicleB1 = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData([
                CustomerMocks.CreateCustomerPf(customerA),
                CustomerMocks.CreateCustomerPf(customerB),
            ])
            .WithData([
                VehicleMocks.CreateVehicle(vehicleA1, customerA, "ABC4D09"),
                VehicleMocks.CreateVehicle(vehicleB1, customerB, "ABC3D72"),
            ])
            .WithData([
                WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerA, vehicleA1, Guid.NewGuid()),
                WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerA, vehicleA1, Guid.NewGuid()),
                WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerB, vehicleB1, Guid.NewGuid()),
            ])
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new GetWorkOrdersRequest { CustomerId = customerA, Page = 1, ItemsPerPage = 10 };
        var response = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        AreEqual(2, response.TotalCount, "Should return only work orders for the specified customer");
        IsTrue(response.Items.All(i => i.CustomerId == customerA));
    }

    [TestMethod("GetList should filter by VehicleId")]
    public async Task GetList_ShouldFilterByVehicleId()
    {
        var customer = Guid.NewGuid();
        var vehicleX = Guid.NewGuid();
        var vehicleY = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(CustomerMocks.CreateCustomerPf(customer));
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleX, Manufacturer = "Make", Model = "X", Color = VehicleColor.White, Year = "2020",
                    LicensePlate = new LicensePlate("JKL3C56"), Chassis = "CH-X", OwnerId = customer,
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleY, Manufacturer = "Make", Model = "Y", Color = VehicleColor.Black, Year = "2021",
                    LicensePlate = new LicensePlate("MNO4D78"), Chassis = "CH-Y", OwnerId = customer,
                });
                ctx.WorkOrders.Add(WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer, vehicleX, Guid.NewGuid()));
                ctx.WorkOrders.Add(WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer, vehicleY, Guid.NewGuid()));
                ctx.WorkOrders.Add(WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer, vehicleX, Guid.NewGuid()));
            })
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new GetWorkOrdersRequest { VehicleId = vehicleX, Page = 1, ItemsPerPage = 10 };
        var response = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        AreEqual(2, response.TotalCount, "Should return only work orders for the specified vehicle");
        IsTrue(response.Items.All(i => i.VehicleId == vehicleX));
    }

    [TestMethod("GetList should return paged list")]
    public async Task GetList_ShouldReturnPaged()
    {
        var customer = Guid.NewGuid();
        var vehicle = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(CustomerMocks.CreateCustomerPf(customer));
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicle, Manufacturer = "Make", Model = "M", Color = VehicleColor.Silver, Year = "2019",
                    LicensePlate = new LicensePlate("PQR5E67"), Chassis = "CH-M", OwnerId = customer,
                });
                for (var i = 0; i < 25; i++)
                    ctx.WorkOrders.Add(WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customer, vehicle, Guid.NewGuid()));
            })
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new GetWorkOrdersRequest { Page = 2, ItemsPerPage = 10 };
        var response = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        AreEqual(25, response.TotalCount);
        AreEqual(10, response.Items.Count());
    }

    [TestMethod("GetList should filter completed")]
    public async Task GetList_ShouldFilterCompleted()
    {
        await using var context = new DbContextTestBuilder()
            .WithData(Enum.GetValues<WorkOrderStatus>().Select(WorkOrderMocks.CreateWorkOrderEntity))
            .Build();

        var budgetService = new BudgetAppService(context, _emailMock, _loggerFactory.CreateLogger<BudgetAppService>());
        var service = new WorkOrderAppService(context, _mapper, _emailMock, _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new GetWorkOrdersRequest { Page = 1, ItemsPerPage = 10 };
        var response = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        AreEqual(4, response.TotalCount);
        AreEqual(4, response.Items.Count());
        AreEqual(WorkOrderStatus.InProgress, response.Items.First().Status);
    }

    #endregion
}
