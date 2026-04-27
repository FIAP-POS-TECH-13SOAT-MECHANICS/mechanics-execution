using Amazon.SQS;
using Amazon.SQS.Model;
using Mechanics.Infra.Messaging.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mechanics.Infra.Messaging.Consumers;

public class ConsumerBackgroundService<T>(
    IAmazonSQS sqsClient,
    QueueUrlResolver urlResolver,
    IServiceScopeFactory scopeFactory,
    ILogger<ConsumerBackgroundService<T>> logger) : BackgroundService where T : class
{
    private string _queueUrl = string.Empty;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _queueUrl = await urlResolver.ResolveAsync<T>(cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20,
            }, stoppingToken);

            if (response.Messages is null)
                continue;

            foreach (var sqsMessage in response.Messages)
                await ProcessMessageAsync(sqsMessage, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(Message sqsMessage, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<T>>();

        try
        {
            logger.LogInformation("Processing message '{MessageId}' for event '{EventType}'",
                sqsMessage.MessageId, typeof(T).Name);

            var message = JsonSerializer.Deserialize<T>(sqsMessage.Body) ??
                          throw new InvalidOperationException($"Failed to deserialize message body to '{typeof(T).Name}'.");

            await consumer.ConsumeAsync(message, cancellationToken);

            await sqsClient.DeleteMessageAsync(_queueUrl, sqsMessage.ReceiptHandle, cancellationToken);

            logger.LogInformation("Message '{MessageId}' for event '{EventType}' processed and deleted",
                sqsMessage.MessageId, typeof(T).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message '{MessageId}' from queue '{QueueUrl}'",
                sqsMessage.MessageId, _queueUrl);
        }
    }
}
