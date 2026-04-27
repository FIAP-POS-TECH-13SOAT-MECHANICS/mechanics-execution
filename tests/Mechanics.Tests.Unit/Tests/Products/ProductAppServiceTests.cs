using AutoMapper;
using Mechanics.Application.Products.Requests;
using Mechanics.Application.Products.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Products;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Products;

[TestClass]
[TestCategory("Products")]
public class ProductAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Products");

    #region consultar produto

    [TestMethod("Consulta produto por Id")]
    public async Task It_ShouldGetProductById()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ProductMocks.CreateProduct(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Products.Add(existing)).Build();
        var handler = new ProductAppService(context, _mapper);

        // Act
        var response = await handler.Get(id, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(id, response.Id);
        Assert.AreEqual(existing.Name, response.Name);
        Assert.AreEqual(existing.Description, response.Description);
        Assert.AreEqual(existing.Quantity, response.Quantity);
        Assert.AreEqual(existing.Status, response.Status);
        Assert.AreEqual(existing.Type, response.Type);
    }

    [TestMethod("Lista de produtos")]
    public async Task It_ShouldListProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductMocks.CreateProduct(Guid.NewGuid()),
            ProductMocks.CreateProduct(Guid.NewGuid()),
        };
        await using var context = new DbContextTestBuilder().WithData(products).Build();
        var handler = new ProductAppService(context, _mapper);
        var request = new GetProductsRequest() { Page = 1, ItemsPerPage = 10 };

        // Act
        var response = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(products.Count, response.Items.Count());
        Assert.AreEqual(products.Count, response.TotalCount);
    }

    #endregion

    #region cadastrar produto

    [TestMethod("Cria produto")]
    public async Task It_ShouldCreateProduct()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new ProductAppService(context, _mapper);
        var request = ProductMocks.BuildCreateRequest();

        // Act
        var response = await handler.Create(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Products.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Name.ToUpper(), created.Name);
        Assert.AreEqual(request.Description.Trim(), created.Description);
        Assert.AreEqual(request.Type, created.Type);
        Assert.AreEqual(request.Quantity, created.Quantity);
        Assert.AreEqual(request.Status, created.Status);
    }


    [TestMethod("Falha ao criar produto")]
    public async Task It_ShouldThrow_WhenCreateProductIsInvalid()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new ProductAppService(context, _mapper);
        var request = ProductMocks.BuildInvalidCreateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Create(request, CancellationToken.None);
        });
    }

    #endregion

    #region atualizar produto

    [TestMethod("Atualiza dados de um produto cadastrado")]
    public async Task It_ShouldUpdateProduct_WhenDataIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ProductMocks.CreateProduct(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Products.Add(existing)).Build();
        var handler = new ProductAppService(context, _mapper);
        var request = ProductMocks.BuildUpdateRequest();

        // Act
        var response = await handler.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.Products.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Name!.ToUpper(), updated.Name);
        Assert.AreEqual(request.Description!.Trim(), updated.Description);
        Assert.AreEqual(request.Quantity!, updated.Quantity);
        Assert.AreEqual(request.Status!, updated.Status);
    }

    [TestMethod("Falha ao atualizar produto inválido")]
    public async Task It_ShouldThrow_WhenUpdateIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ProductMocks.CreateInvalidProduct(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Products.Add(existing)).Build();
        var handler = new ProductAppService(context, _mapper);
        var request = ProductMocks.BuildInvalidUpdateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Update(id, request, CancellationToken.None);
        });
    }

    #endregion

    #region deletar produto

    [TestMethod("Deleta um produto por id")]
    public async Task It_ShouldDeleteProductById()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ProductMocks.CreateProduct(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Products.Add(existing)).Build();
        var handler = new ProductAppService(context, _mapper);

        // Act
        var response = await handler.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsTrue(response);
    }

    [TestMethod("Falha ao deletar um pedido")]
    public async Task It_ShouldThrow_WhenDeleteIdIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistingId = Guid.NewGuid();
        var existing = ProductMocks.CreateProduct(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Products.Add(existing)).Build();
        var handler = new ProductAppService(context, _mapper);

        // Act
        var response = await handler.Delete(nonExistingId, CancellationToken.None);

        // Assert
        Assert.IsFalse(response);
    }

    #endregion
}
