using Mechanics.Domain.ServicesCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class ServiceCatalogConfiguration : IEntityTypeConfiguration<ServiceCatalog>
{
    public void Configure(EntityTypeBuilder<ServiceCatalog> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(100);
        builder.HasIndex(entity => entity.Name).IsUnique();

        builder.Property(entity => entity.Description).HasMaxLength(255);

        builder.Property(entity => entity.BasePrice).HasPrecision(18, 2);
    }
}
