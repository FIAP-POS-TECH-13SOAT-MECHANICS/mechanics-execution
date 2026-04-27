using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Mechanics.Infra.Messaging.Consumers;
using Mechanics.Infra.Messaging.Helpers;
using Mechanics.Infra.Messaging.Options;
using Mechanics.Infra.Messaging.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mechanics.Infra.Messaging.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    ///     Adiciona serviços relacionados à Messaging ao container de injeção de dependência.
    /// </summary>
    /// <param name="services">Container de injeção de dependência.</param>
    /// <param name="configure">Configuração de consumidores de mensagens.</param>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        Action<MessagingBuilder>? configure = null)
    {
        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsCredentialsOptions>>().Value;

            if (options.UseLocalstack)
                return new AmazonSQSClient(
                    new BasicAWSCredentials("local", "empty-key"),
                    new AmazonSQSConfig
                    {
                        RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
                        ServiceURL = options.LocalstackUrl,
                    });

            return new AmazonSQSClient(
                new SessionAWSCredentials(options.AccessKey, options.SecretAccessKey, options.SessionToken),
                new AmazonSQSConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region) });
        });

        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<QueueUrlResolver>();

        var builder = new MessagingBuilder(services);
        configure?.Invoke(builder);

        return services;
    }
}

public class MessagingBuilder(IServiceCollection services)
{
    public MessagingBuilder AddConsumer<TConsumer, TEvent>() where TConsumer : class, IEventConsumer<TEvent> where TEvent : class
    {
        services.AddScoped<IEventConsumer<TEvent>, TConsumer>();
        services.AddHostedService<ConsumerBackgroundService<TEvent>>();

        return this;
    }
}
