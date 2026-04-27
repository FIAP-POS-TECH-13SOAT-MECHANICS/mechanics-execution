using Mechanics.Application.Auth.Requests;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Notification;

[TestClass]
[TestCategory("Notification")]
public class ResetPasswordTest(TestContext testContext)
{
    [TestMethod("Deve enviar e-mail de recuperação de senha")]
    public async Task It_ShouldSendPasswordResetEmail()
    {
        var client = TestProperties.Factory.CreateClient();
        var emailsCount = await GetEmailsCount(testContext.CancellationTokenSource.Token);

        var request = new ResetPasswordRequest { CpfNumber = "12345678909" };
        var response = await client.PostAsJsonAsync("api/auth/reset-password", request, testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsGreaterThan(emailsCount, await GetEmailsCount(testContext.CancellationTokenSource.Token));
    }

    private static async Task<int> GetEmailsCount(CancellationToken cancellationToken)
    {
        var client = new HttpClient { BaseAddress = TestProperties.GetEmailClientUri() };

        var response = await client.GetAsync("api/v1/info", cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EmailClientInfoResponse>(cancellationToken: cancellationToken))!.Messages;
    }

    public class EmailClientInfoResponse
    {
        public required int Messages { get; init; }
    }
}
