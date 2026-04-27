using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Customers;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Customers")]
public class CustomersControllerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod("Cadastro de cliente individual")]
    public async Task It_ShouldCreateIndividualCustomer()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateIndividualCustomerRequest
        {
            FullName = "Joao da Silva",
            Email = "joao@example.com",
            CpfNumber = "294.604.050-02",
        };

        // Act
        var httpResponse =
            await client.PostAsJsonAsync("api/customers/individual", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Cadastro de cliente empresarial")]
    public async Task It_ShouldCreateBusinessCustomer()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateBusinessCustomerRequest
        {
            CompanyName = "Empresa XYZ Ltda",
            CnpjNumber = "72.933.819/0001-12",
            ResponsibleFullName = "Responsavel XYZ",
            ResponsibleEmail = "contato@xyz.com",
            ResponsibleCpfNumber = "225.405.130-00",
        };

        // Act
        var httpResponse =
            await client.PostAsJsonAsync("api/customers/business", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Cadastro de cliente PJ com CNPJ inválido")]
    public async Task It_ShouldReturnBadRequest_WhenCnpjIsInvalid()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var invalidRequest = new CreateBusinessCustomerRequest
        {
            CompanyName = "Empresa XYZ Ltda",
            CnpjNumber = "34.444.828/0001-21",
            ResponsibleFullName = "Responsavel XYZ",
            ResponsibleEmail = "contato@xyz.com",
            ResponsibleCpfNumber = "770.259.350-42",
        };

        // Act
        var httpResponse =
            await client.PostAsJsonAsync("api/customers/business", invalidRequest, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
}
