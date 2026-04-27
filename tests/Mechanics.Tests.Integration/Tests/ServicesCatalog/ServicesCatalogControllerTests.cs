using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.ServicesCatalog;

[TestClass]
[TestCategory("Integration")]
[TestCategory("ServicesCatalog")]
public class ServiceCatalogControllerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod("Verifica se rota está acessível")]
    public async Task It_ShouldReachServiceCatalogEndpoint()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        // Act
        var response = await client.GetAsync("api/service-catalog", TestContext.CancellationTokenSource.Token);

        // Assert
        Console.WriteLine($"Status: {response.StatusCode}");
        Assert.AreNotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }


    [TestMethod("Cadastro de serviço")]
    public async Task It_ShouldCreateServiceCatalog()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateServiceCatalogRequest
        {
            Name = $"Alinhamento {Guid.NewGuid():N}",
            Description = "Serviço de alinhamento de rodas",
            BasePrice = 150.00m,
            AverageTime = 40,
            Status = ServiceCatalogStatusType.Active,
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Falha ao cadastrar serviço com nome duplicado")]
    public async Task It_ShouldFailToCreateServiceCatalog_WhenNameIsDuplicated()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var name = $"Balanceamento {Guid.NewGuid():N}";

        // primeiro cadastro (deve funcionar)
        var request = new CreateServiceCatalogRequest
        {
            Name = name,
            Description = "Balanceamento de rodas dianteiras",
            BasePrice = 199.90m,
            AverageTime = 45,
            Status = ServiceCatalogStatusType.Active,
        };

        // segundo cadastro com mesmo nome (deve falhar)
        var duplicateRequest = new CreateServiceCatalogRequest
        {
            Name = name, // mesmo nome
            Description = "Outro serviço com nome repetido",
            BasePrice = 149.90m,
            AverageTime = 30,
            Status = ServiceCatalogStatusType.Active,
        };

        // Act
        var firstResponse = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse =
            await client.PostAsJsonAsync("api/service-catalog", duplicateRequest, TestContext.CancellationTokenSource.Token);

        var raw = await secondResponse.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);
        Console.WriteLine($"Status: {secondResponse.StatusCode}, Body: {raw}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, secondResponse.StatusCode,
            $"Esperado BadRequest, mas veio: {secondResponse.StatusCode}");
        Assert.IsTrue(raw.Contains("Service name must be unique.", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod("Busca por termo relacionado ao tipo de veículo")]
    public async Task It_ShouldReturnServices_WhenSearchingByVehicleTypeTerm()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var description = "Geometria para SUV e balanceamento de rodas";
        var request = new CreateServiceCatalogRequest
        {
            Name = $"Serviço SUV {Guid.NewGuid():N}",
            Description = description,
            BasePrice = 180.00m,
            AverageTime = 50,
            Status = ServiceCatalogStatusType.Active,
        };

        var postResponse = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);

        // Act
        var searchResponse =
            await client.GetAsync("api/service-catalog/search?term=SUV", TestContext.CancellationTokenSource.Token);
        var content = await searchResponse.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);

        // Assert
        Console.WriteLine($"Status: {searchResponse.StatusCode}, Body: {content}");
        Assert.AreEqual(HttpStatusCode.OK, searchResponse.StatusCode);
        Assert.IsTrue(content.Contains("SUV", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod("Busca com termo inexistente")]
    public async Task It_ShouldReturnEmptyList_WhenSearchTermDoesNotMatch()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        // Act
        var response = await client.GetAsync("api/service-catalog/search?term=xyz-inexistente",
            TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);
        Console.WriteLine($"Status: {response.StatusCode}, Body: {content}");
        Assert.IsTrue(content.Contains("\"count\":0") || content.Contains("\"items\":[]"));
    }

    [TestMethod("Listagem paginada de serviços")]
    public async Task It_ShouldListServicesWithPagination()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        for (var i = 0; i < 15; i++)
        {
            var request = new CreateServiceCatalogRequest
            {
                Name = $"Serviço Paginado {Guid.NewGuid():N}",
                Description = "Serviço para teste de paginação",
                BasePrice = 100 + i,
                AverageTime = 30 + i,
                Status = ServiceCatalogStatusType.Active,
            };

            var response = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        // Act
        var pagedResponse =
            await client.GetAsync("api/service-catalog?page=2&itemsPerPage=10", TestContext.CancellationTokenSource.Token);
        var content = await pagedResponse.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);

        // Assert
        Console.WriteLine($"Status: {pagedResponse.StatusCode}, Body: {content}");
        Assert.AreEqual(HttpStatusCode.OK, pagedResponse.StatusCode);
        Assert.Contains("\"items\"", content);
    }
}
