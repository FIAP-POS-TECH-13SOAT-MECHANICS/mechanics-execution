using DotNet.Testcontainers.Containers;
using Testcontainers.LocalStack;

namespace Mechanics.Tests.Integration.Helpers;

public class TestAwsClientContainer : IAsyncDisposable
{
    public IContainer Container { get; } = new LocalStackBuilder("localstack/localstack:3")
        .WithName($"testcontainers-aws-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Container.DisposeAsync();
    }
}
