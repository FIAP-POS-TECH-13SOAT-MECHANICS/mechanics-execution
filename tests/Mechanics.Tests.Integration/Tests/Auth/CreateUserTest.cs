using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class CreateUserTest(TestContext testContext)
{
    [TestMethod("Falha quando RoleId é inválido.")]
    public async Task It_ShouldFail_WhenRoleIdIsInvalid()
    {
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateUserRequest
        {
            FullName = "MARIA FERNANDA SOUZA",
            CpfNumber = "50700421068",
            Email = "maria.souza@mechanics.com",
            RoleId = new Guid("f2d59afa-6e85-4557-8ff1-733343ba83f8"),
        };

        var response = await client.PostAsJsonAsync("api/auth/users", request,
            testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod("Sucesso ao criar usuário com CPF formatado.")]
    public async Task It_ShouldCreateUser_WhenCpfIsFormatted()
    {
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateUserRequest
        {
            FullName = "Mário Fernando Silva",
            CpfNumber = "507.004.210-68",
            Email = "mario.silva@mechanics.com",
            RoleId = new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"),
        };

        var response = await client.PostAsJsonAsync("api/auth/users", request,
            testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
