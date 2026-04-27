using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Mechanics.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.WorkOrders;

[TestClass]
public class WorkOrdersControllerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Create_And_RequestApproval_Flow_Works()
    {
        var client = TestProperties.Factory.GetAuthenticatedClient(RoleNames.Attendant);

        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Customers.Add(new Customer
            {
                Id = customerId,
                Name = "Integration Customer",
                Email = "int.customer@example.com",
                Document = new PersonalDocument(DocumentType.Cpf, "12345678909"),
            });
            db.Vehicles.Add(new Vehicle
            {
                Id = vehicleId,
                Manufacturer = "Make",
                Model = "Model",
                Color = VehicleColor.Black,
                Year = "2020",
                LicensePlate = new LicensePlate("INT1234"),
                Chassis = "CHINT",
                OwnerId = customerId,
            });
            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var createReq = new CreateWorkOrderRequest
        {
            VehicleId = vehicleId,
            ReportedProblem = "Teste integração",
        };

        var createResp = await client.PostAsJsonAsync("/api/work-orders", createReq, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);

        var createdBody =
            await createResp.Content.ReadFromJsonAsync<Dictionary<string, Guid>>(TestContext.CancellationTokenSource.Token);
        if (createdBody is null)
        {
            Assert.Fail("Response body desserializado é nulo.");
            return;
        }

        Assert.IsTrue(createdBody.ContainsKey("createdId"));
        var woId = createdBody["createdId"];

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var svcId = Guid.NewGuid();
            var svc = new ServiceCatalog
            {
                Id = svcId,
                Name = "Teste Serviço",
                Description = "Serviço usado no teste de integração",
                BasePrice = 100m,
                AverageTime = 30,
                Status = ServiceCatalogStatusType.Active,
            };

            db.ServiceCatalog.Add(svc);

            var wo = await db.WorkOrders.Where(workOrder => workOrder.Id == woId)
                .Include(workOrder => workOrder.ServiceCatalog)
                .FirstOrDefaultAsync(TestContext.CancellationTokenSource.Token);
            if (wo is null)
                Assert.Fail("WorkOrder not found in DB after creation.");

            wo.ServiceCatalog ??= new List<ServiceCatalog>();
            wo.ServiceCatalog.Add(svc);

            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var attendantUserId = new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7");

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var role = await db.Roles.FirstAsync(r => r.Name == RoleNames.Attendant,
                TestContext.CancellationTokenSource.Token);

            var existingUser = await db.Users.FindAsync([attendantUserId], TestContext.CancellationTokenSource.Token);
            if (existingUser == null)
            {
                db.Users.Add(new User
                {
                    Id = attendantUserId,
                    FullName = "Integration Attendant",
                    CpfNumber = "60975754084",
                    Email = "int.attendant@mechanics.com",
                    PasswordHash = "hash",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = role.Id,
                    CreationDate = DateTime.UtcNow,
                });
            }

            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var reqApprovalResp = await client.PostAsync($"/api/work-orders/{woId}/request-approval", null,
            TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.NoContent, reqApprovalResp.StatusCode);

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wo = await db.WorkOrders.FindAsync([woId], TestContext.CancellationTokenSource.Token);
            Assert.IsNotNull(wo);
            Assert.AreEqual(WorkOrderStatus.PendingApproval, wo.Status);
            Assert.IsNotNull(wo.ApprovalRequestedAt);
            Assert.AreEqual(attendantUserId, wo.LastStatusChangeBy);
        }
    }
}
