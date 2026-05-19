using Mechanics.Infra.Messaging.Publishers;
using System.Collections.Concurrent;

namespace Mechanics.Tests.Unit.Helpers;

public class TestEventPublisher : IEventPublisher
{
    private readonly ConcurrentBag<object> _messages = new();

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        _messages.Add(message!);
        return Task.CompletedTask;
    }

    public IEnumerable<T> GetPublished<T>() where T : class => _messages.OfType<T>();
}
