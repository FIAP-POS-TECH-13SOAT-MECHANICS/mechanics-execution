using AutoMapper;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.ServicesCatalog.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Base.Validation;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.ServicesCatalog;

[TestClass]
[TestCategory("ServicesCatalog")]
public class ServiceCatalogAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("ServicesCatalog");

    #region cadastrar serviço

    [TestMethod("Cadastro de serviço")]
    public async Task It_ShouldCreateService()
    {
        // Arrange
        var context = new DbContextTestBuilder().Build();
        var service = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildCreateRequest();

        // Act
        var response = await service.Create(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);

        var created = await context.ServiceCatalog.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(created);
        Assert.AreEqual(request.Name.ToUpper(), created.Name);
        Assert.AreEqual(request.Description.Trim(), created.Description);
        Assert.AreEqual(request.BasePrice, created.BasePrice);
        Assert.AreEqual(request.AverageTime, created.AverageTime);
        Assert.AreEqual(request.Status ?? created.Status, created.Status);
    }

    [TestMethod("Cadastro de serviço inválido")]
    public async Task It_ShouldThrow_WhenServiceIsInvalid()
    {
        // Arrange
        var context = new DbContextTestBuilder().Build();
        var service = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildInvalidCreateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await service.Create(request, CancellationToken.None);
        });
    }

    #endregion

    #region atualizar serviço

    [TestMethod("Alteração de serviço")]
    public async Task It_ShouldUpdateService()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ServicesCatalogMocks.CreateService(id);
        var context = new DbContextTestBuilder().WithData(ctx => ctx.ServiceCatalog.Add(existing)).Build();
        var service = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildUpdateRequest();

        // Act
        var response = await service.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);

        var updated = await context.ServiceCatalog.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Name!.Trim().ToUpper(), updated.Name);
        Assert.AreEqual(request.Description, updated.Description);
        Assert.AreEqual(request.BasePrice, updated.BasePrice);
        Assert.AreEqual(request.AverageTime, updated.AverageTime);
        Assert.AreEqual(request.Status, updated.Status);
    }

    #endregion

    #region consultar serviço

    [TestMethod("Consulta por ID")]
    public async Task It_ShouldGetServiceById()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ServicesCatalogMocks.CreateService(id);
        var context = new DbContextTestBuilder().WithData(ctx => ctx.ServiceCatalog.Add(existing)).Build();
        var service = new ServiceCatalogAppService(context, _mapper);

        // Act
        var response = await service.Get(id, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(id, response.Id);
        Assert.AreEqual(existing.Name, response.Name);
        Assert.AreEqual(existing.Description, response.Description);
    }

    [TestMethod("Listar por nome")]
    public async Task It_ShouldListByName()
    {
        // Arrange
        var context = new DbContextTestBuilder()
            .WithData([
                ServicesCatalogMocks.CreateService(Guid.NewGuid(), "Freios"),
                ServicesCatalogMocks.CreateService(Guid.NewGuid(), "Suspensão"),
                ServicesCatalogMocks.CreateService(Guid.NewGuid(), "Troca de óleo"),
            ])
            .Build();

        var service = new ServiceCatalogAppService(context, _mapper);
        var request = new GetServiceCatalogRequest { Name = "freios", Page = 1, ItemsPerPage = 10 };

        // Act
        var list = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Items.Count());
        Assert.AreEqual(1, list.TotalCount);
        Assert.Contains("FREIOS", list.Items.First().Name.ToUpper());
    }

    [TestMethod("Busca por termo relacionado ao tipo de veículo")]
    public async Task It_ShouldSearchByVehicleTypeTerm()
    {
        // Arrange
        var context = new DbContextTestBuilder()
            .WithData([
                ServicesCatalogMocks.CreateSearchableService(Guid.NewGuid(), "Geometria", "Geometria para SUV e pneus"),
                ServicesCatalogMocks.CreateSearchableService(Guid.NewGuid(), "Troca de Óleo", "Óleo sintético para SUV"),
            ])
            .Build();

        var service = new ServiceCatalogAppService(context, _mapper);

        // Act
        var result = await service.GetSearch("SUV", TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Any());
        Assert.IsTrue(result.Items.All(s =>
            s.Name.Contains("SUV", StringComparison.OrdinalIgnoreCase) ||
            s.Description.Contains("SUV", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod("Busca textual por nome ou descrição")]
    public async Task It_ShouldSearchByTerm()
    {
        // Arrange
        var context = new DbContextTestBuilder()
            .WithData([
                ServicesCatalogMocks.CreateSearchableService(Guid.NewGuid(), "Freios", "Serviço de freios e pastilhas"),
                ServicesCatalogMocks.CreateSearchableService(Guid.NewGuid(), "Suspensão", "Revisão completa da suspensão"),
            ])
            .Build();

        var service = new ServiceCatalogAppService(context, _mapper);

        // Act
        var result = await service.GetSearch("freios", TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Items.Count());
        Assert.IsTrue(result.Items.All(s =>
            s.Name.Contains("freios", StringComparison.OrdinalIgnoreCase) ||
            s.Description.Contains("freios", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod("Paginação de serviços")]
    public async Task It_ShouldPaginateServiceCatalog()
    {
        // Arrange
        var context = new DbContextTestBuilder()
            .WithData(ServicesCatalogMocks.BuildManyServices(20))
            .Build();

        var service = new ServiceCatalogAppService(context, _mapper);
        var request = new GetServiceCatalogRequest
        {
            Page = 2,
            ItemsPerPage = 10,
        };

        // Act
        var result = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Items.Count());
        Assert.AreEqual(20, result.TotalCount);
    }

    #endregion

    #region excluir serviço

    [TestMethod("Remover serviço")]
    public async Task It_ShouldDeleteService()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ServicesCatalogMocks.CreateService(id);
        var context = new DbContextTestBuilder().WithData(ctx => ctx.ServiceCatalog.Add(existing)).Build();
        var service = new ServiceCatalogAppService(context, _mapper);

        // Act
        var result = await service.Delete(id, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsTrue(result);

        var deleted = await context.ServiceCatalog.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, TestContext.CancellationTokenSource.Token);

        Assert.IsNull(deleted);
    }

    [TestMethod("Falha ao remover serviço inexistente")]
    public async Task It_ShouldFailToDeleteNonexistentService()
    {
        // Arrange
        var context = new DbContextTestBuilder().Build();
        var service = new ServiceCatalogAppService(context, _mapper);
        var id = Guid.NewGuid();

        // Act
        var result = await service.Delete(id, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion
}
