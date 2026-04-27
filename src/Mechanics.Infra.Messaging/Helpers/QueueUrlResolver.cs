using Amazon.SQS;
using Mechanics.Infra.Messaging.Options;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;

namespace Mechanics.Infra.Messaging.Helpers;

public class QueueUrlResolver(IAmazonSQS sqsClient, IOptions<MessagingOptions> options)
{
    private readonly Dictionary<string, string> _queueNames = options.Value.QueueNames;
    private readonly ConcurrentDictionary<Type, Lazy<Task<string>>> _urls = new();

    public Task<string> ResolveAsync<T>(CancellationToken cancellationToken) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _urls.GetOrAdd(typeof(T), _ => new Lazy<Task<string>>(
            () => FetchUrlAsync<T>(CancellationToken.None))).Value;
    }

    private async Task<string> FetchUrlAsync<T>(CancellationToken cancellationToken) where T : class
    {
        var eventName = typeof(T).Name.Replace("Event", string.Empty);

        if (!_queueNames.TryGetValue(eventName, out var queueName))
            throw new InvalidOperationException($"Queue not configured for event '{eventName}'.");

        var response = await sqsClient.GetQueueUrlAsync(queueName, cancellationToken);

        return response.HttpStatusCode != HttpStatusCode.OK
            ? throw new InvalidOperationException($"Error fetching URL for queue '{queueName}'.")
            : response.QueueUrl;
    }
}
