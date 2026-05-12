using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Testcontainers.LocalStack;

namespace Mechanics.Tests.Integration.Helpers;

public class TestAwsClientContainer : IAsyncDisposable
{
    public IContainer Container { get; } = new LocalStackBuilder("localstack/localstack:3")
        .WithName($"testcontainers-aws-{Guid.NewGuid()}")
        .WithResourceMapping(Path.Combine(AppContext.BaseDirectory, "scripts", "localstack"),
            "/etc/localstack/init/ready.d", 0, 0,
            UnixFileModes.UserRead | UnixFileModes.UserExecute | UnixFileModes.GroupRead | UnixFileModes.GroupExecute)
        .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Ready."))
        .WithCleanUp(true)
        .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Container.DisposeAsync();
    }
}
