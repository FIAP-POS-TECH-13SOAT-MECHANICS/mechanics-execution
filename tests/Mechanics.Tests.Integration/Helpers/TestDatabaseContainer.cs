using Testcontainers.MsSql;

namespace Mechanics.Tests.Integration.Helpers;

public class TestDatabaseContainer : IAsyncDisposable
{
    public MsSqlContainer Container { get; } = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("b0I6h9G%1zJo")
        .WithEnvironment("MSSQL_PID", "Express")
        .WithName($"testcontainers-db-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Container.DisposeAsync();
    }
}
