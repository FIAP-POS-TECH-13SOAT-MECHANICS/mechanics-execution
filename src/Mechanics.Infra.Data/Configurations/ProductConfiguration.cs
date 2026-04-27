using Mechanics.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(100);
        builder.HasIndex(entity => entity.Name).IsUnique();

        builder.Property(entity => entity.Description).HasMaxLength(255);
        builder.Property(entity => entity.UnitPrice).HasPrecision(18, 2);
    }
}
