using Amazon.SQS;
using Mechanics.Api;
using Mechanics.Infra.Messaging.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Mechanics.Tests.Integration.Helpers;

public class ApplicationFactory : WebApplicationFactory<Program>
{
    private readonly ConcurrentDictionary<string, string?> _tokens = new();
    private readonly RSA _rsa = RSA.Create();

    public HttpClient GetAuthenticatedClient(string roleName)
    {
        var authenticatedClient = CreateClient();
        var token = _tokens.GetOrAdd(roleName, _ => GetToken());
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return authenticatedClient;

        string GetToken()
        {
            var tokenGenerator = new TestTokenGenerator(_rsa);
            return tokenGenerator.GenerateAccessTokenByRoleName(roleName);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "keys"));
        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "keys", "jwt-public.pem");
        File.WriteAllText(publicKeyPath, _rsa.ExportRSAPublicKeyPem());

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAmazonSQS));
            if (descriptor != null)
                services.Remove(descriptor);

            var client = TestProperties.GetSqsClient();
            services.AddSingleton<IAmazonSQS>(client);

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<MessagingOptions>>();
            foreach (var queue in options.Value.QueueNames.Values)
                client.CreateQueueAsync(queue, CancellationToken.None).GetAwaiter().GetResult();
        });

        base.ConfigureWebHost(builder);
    }
}
