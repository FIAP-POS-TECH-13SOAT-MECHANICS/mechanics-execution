namespace Mechanics.Infra.Messaging.Consumers;

public interface IEventConsumer<in T> where T : class
{
    Task ConsumeAsync(T message, CancellationToken cancellationToken = default);
}
