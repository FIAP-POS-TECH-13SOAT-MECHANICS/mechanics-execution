using Mechanics.Domain.Auth;
using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace Mechanics.Infra.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ServiceCatalog> ServiceCatalog { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderHistory> WorkOrderHistories { get; set; }
    public DbSet<Budget> Budgets { get; set; } = default!;
    public DbSet<BudgetItem> BudgetItems { get; set; } = default!;
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Mechanics");

        ConfigureAbstractEntities(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .AddInterceptors(new NormalizationInterceptor());

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    ///     Configuração dos atributos padrão nas entidades do domínio.
    /// </summary>
    private static void ConfigureAbstractEntities(ModelBuilder modelBuilder)
    {
        var types = modelBuilder.Model.GetEntityTypes()
            .Where(type => typeof(AbstractEntity).IsAssignableFrom(type.ClrType))
            .Select(type => type.ClrType);

        foreach (var type in types)
        {
            modelBuilder.Entity(type)
                .Property(nameof(AbstractEntity.Id))
                .HasDefaultValueSql("NEWID()")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity(type)
                .Property(nameof(AbstractEntity.CreationDate))
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIME()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
