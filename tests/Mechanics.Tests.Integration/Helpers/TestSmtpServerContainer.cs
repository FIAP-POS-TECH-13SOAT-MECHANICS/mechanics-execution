using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Mechanics.Tests.Integration.Helpers;

public class TestSmtpServerContainer : IAsyncDisposable
{
    public const string UserName = "rdFpp6iP@mechanics.com";
    public const string Password = "ojS$OD0AUly";

    public IContainer Container { get; } = new ContainerBuilder("axllent/mailpit:v1.27.10")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithEnvironment("MP_SMTP_AUTH", $"{UserName}:{Password}")
        .WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "true")
        .WithName($"testcontainers-smtp-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Container.DisposeAsync();
    }
}
