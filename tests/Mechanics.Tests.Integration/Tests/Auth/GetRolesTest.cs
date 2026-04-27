using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Roles")]
public class GetRolesTest(TestContext testContext)
{
    [TestMethod("Deve retornar uma lista não vazia")]
    public async Task It_ShouldReturnNonEmptyList()
    {
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var response = await client.GetAsync("api/auth/roles", testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(response.Content);
        var content = await response.Content.ReadFromJsonAsync<GetRolesResponse>(testContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.IsNotEmpty(content.Items);
    }

    [TestMethod("Deve retornar 403 se não for administrador")]
    public async Task It_ShouldReturnForbiddenIfNotAdministrator()
    {
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Attendant);

        var response = await client.GetAsync("api/auth/roles", testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
