using AutoMapper;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Customers.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Customers;

[TestClass]
[TestCategory("Customers")]
public class CustomerAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Customers");

    #region cadastrar cliente

    [TestMethod("Cria cliente pessoa física (CPF)")]
    public async Task It_ShouldCreateIndividualCustomer_WhenCpfIsValid()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);
        var request = CustomerMocks.BuildCreateRequestPf();

        // Act
        var response = await handler.CreateIndividual(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.FullName.Trim().ToUpper(), created.Name);
        Assert.AreEqual(request.Email.Trim().ToLower(), created.Email);
        Assert.AreEqual(DocumentType.Cpf, created.Document.Type);
    }

    [TestMethod("Cria cliente pessoa jurídica (CNPJ)")]
    public async Task It_ShouldCreateBusinessCustomer_WhenCnpjIsValid()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);
        var request = CustomerMocks.BuildCreateRequestPj();

        // Act
        var response = await handler.CreateBusiness(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(DocumentType.Cnpj, created.Document.Type);
    }

    [TestMethod("Falha ao criar cliente individual inválido")]
    public async Task It_ShouldThrow_WhenCreateIndividualCustomerIsInvalid()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);
        var request = CustomerMocks.BuildInvalidCreateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.CreateIndividual(request, CancellationToken.None);
        });
    }

    #endregion

    #region atualizar cliente

    [TestMethod("Atualiza dados do cliente cadastrado")]
    public async Task It_ShouldUpdateCustomer_WhenDataIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = CustomerMocks.CreateCustomerPf(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Customers.Add(existing)).Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);
        var request = CustomerMocks.BuildUpdateRequest();

        // Act
        var response = await handler.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Name!.Trim().ToUpper(), updated.Name);
        Assert.AreEqual(request.Email!.Trim().ToLower(), updated.Email);
        Assert.AreEqual(request.Document!.Type, updated.Document.Type);
        Assert.AreEqual(request.Document!.Number, updated.Document.Number);
    }

    [TestMethod("Falha ao atualizar cliente inválido")]
    public async Task It_ShouldThrow_WhenUpdateIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = CustomerMocks.CreateCustomerPf(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Customers.Add(existing)).Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);
        var request = CustomerMocks.BuildInvalidUpdateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Update(id, request, CancellationToken.None);
        });
    }

    #endregion

    #region consultar cliente

    [TestMethod("Consulta cliente por Id")]
    public async Task It_ShouldGetCustomerById()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = CustomerMocks.CreateCustomerPf(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.Customers.Add(existing)).Build();
        var handler = new CustomerAppService(context,  new NullEventPublisher(), _mapper);

        // Act
        var response = await handler.Get(id, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(id, response.Id);
        Assert.AreEqual(existing.Name, response.Name);
        Assert.AreEqual(existing.Email, response.Email);
        Assert.AreEqual(existing.Document.Number, response.Document.Number);
    }

    [TestMethod("Lista de clientes")]
    public async Task It_ShouldListCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CustomerMocks.CreateCustomerPf(Guid.NewGuid()),
            CustomerMocks.CreateCustomerPj(Guid.NewGuid()),
        };
        await using var context = new DbContextTestBuilder().WithData(customers).Build();
        var handler = new CustomerAppService(context, new NullEventPublisher(), _mapper);
        var request = new GetCustomersRequest { Page = 1, ItemsPerPage = 10 };

        // Act
        var response = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(customers.Count, response.Items.Count());
        Assert.AreEqual(customers.Count, response.TotalCount);
    }

    #endregion
}
