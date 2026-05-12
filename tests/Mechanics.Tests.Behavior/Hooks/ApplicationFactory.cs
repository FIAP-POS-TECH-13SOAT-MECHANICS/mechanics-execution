using Amazon.SQS;
using Mechanics.Api;
using Mechanics.Infra.Data;
using Mechanics.Infra.Integrations.EmailSender;
using Mechanics.Infra.Messaging.Publishers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Mechanics.Tests.Behavior.Hooks;

public class ApplicationFactory : WebApplicationFactory<Program>
{
    private readonly RSA _rsa = RSA.Create();

    public HttpClient GetAuthenticatedClient(string roleName)
    {
        var client = CreateClient();
        var token = new TestTokenGenerator(_rsa).GenerateAccessTokenByRoleName(roleName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "keys"));
        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "keys", "jwt-public.pem");
        File.WriteAllText(publicKeyPath, _rsa.ExportRSAPublicKeyPem());

        builder.ConfigureServices(services =>
        {
            services.UseInMemoryDbContext("mechanics-behavior")
                .UseMockedMessaging()
                .UseMockedEmailSender();
        });

        Environment.SetEnvironmentVariable("AppInfo__RoutePrefix", "api");
        Environment.SetEnvironmentVariable("Datadog__OtlpEndpoint", "http://localhost");

        base.ConfigureWebHost(builder);
    }
}

internal static class Extensions
{
    public static IServiceCollection UseInMemoryDbContext(this IServiceCollection services, string databaseName)
    {
        var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (descriptor is not null)
            services.Remove(descriptor);

        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));

        return services;
    }

    public static IServiceCollection UseMockedMessaging(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IAmazonSQS));
        if (descriptor is not null)
            services.Remove(descriptor);
        services.AddSingleton(new Mock<IAmazonSQS>().Object);

        var consumerServices = services
            .Where(s => s.ImplementationType?.Name.Contains("ConsumerBackgroundService") == true)
            .ToList();
        foreach (var service in consumerServices)
            services.Remove(service);

        var publisherDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IEventPublisher));
        if (publisherDescriptor is not null)
            services.Remove(publisherDescriptor);
        services.AddSingleton(new Mock<IEventPublisher>().Object);

        return services;
    }

    public static IServiceCollection UseMockedEmailSender(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IEmailSenderService));
        if (descriptor is not null)
            services.Remove(descriptor);
        services.AddSingleton(new Mock<IEmailSenderService>().Object);

        return services;
    }
}
