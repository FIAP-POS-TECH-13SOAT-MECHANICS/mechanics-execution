using Mechanics.Infra.Messaging.Publishers;

namespace Mechanics.Tests.Unit.Helpers;

public class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
}
