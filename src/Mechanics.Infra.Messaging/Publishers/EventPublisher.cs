using Amazon.SQS;
using Amazon.SQS.Model;
using Mechanics.Infra.Messaging.Helpers;
using System.Text.Json;

namespace Mechanics.Infra.Messaging.Publishers;

public class EventPublisher(IAmazonSQS sqsClient, QueueUrlResolver urlResolver) : IEventPublisher
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var queueUrl = await urlResolver.ResolveAsync<T>(cancellationToken);

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
        }, cancellationToken);
    }
}
