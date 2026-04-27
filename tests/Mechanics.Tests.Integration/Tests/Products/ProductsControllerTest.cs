using Mechanics.Application.Products.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Products;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Products;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Products")]
public class ProductsControllerTest
{
    public TestContext TestContext { get; set; }

    [TestMethod("Cadastro de produto")]
    public async Task It_ShouldCreateProduct()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateProductRequest
        {
            Name = "Pneu",
            Description = "Pneu Pirelli",
            Type = ProductType.Part,
            Quantity = 4,
            Status = ProductStatusType.Active,
            UnitPrice = 199,
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/products", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Cadastro de produto com erro")]
    public async Task Ir_ShouldReturnBadRequest_WhenQuantityIsLessThenZero()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var invalidRequest = new CreateProductRequest
        {
            Name = "Pneu",
            Description = "Pneu Pirelli",
            Type = ProductType.Part,
            Quantity = -1,
            Status = ProductStatusType.Active,
            UnitPrice = 199m,
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/products", invalidRequest, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
}
