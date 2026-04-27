using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Helpers;

/// <summary>
///     Builder para criação de instâncias do AppDbContext para testes.
/// </summary>
public class DbContextTestBuilder
{
    private readonly string _dbName = $"Tests_{Guid.NewGuid()}";
    private readonly List<Action<AppDbContext>> _seeders = [];

    /// <summary>
    ///     Adiciona um seeder de dados que será executado antes de retornar o contexto.
    /// </summary>
    public DbContextTestBuilder WithData(Action<AppDbContext> seeder)
    {
        _seeders.Add(seeder);
        return this;
    }

    /// <inheritdoc cref="WithData(Action{AppDbContext})" />
    public DbContextTestBuilder WithData<T>(IEnumerable<T> seeder) where T : class
    {
        _seeders.Add(context => context.AddRange(seeder));
        return this;
    }

    /// <summary>
    ///     Cria uma nova instância do AppDbContext InMemory e aplica os seeders configurados.
    /// </summary>
    public AppDbContext Build()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        var context = new AppDbContext(options);

        foreach (var seeder in _seeders)
        {
            seeder(context);
            context.SaveChanges();
        }

        return context;
    }
}
