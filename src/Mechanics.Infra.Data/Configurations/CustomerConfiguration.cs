using Mechanics.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(100);
        builder.HasIndex(entity => entity.Name);

        builder.Property(entity => entity.Email).HasMaxLength(100);
        builder.HasIndex(entity => entity.Email).IsUnique();

        builder.OwnsOne(entity => entity.Document, document =>
        {
            document.Property(d => d.Number).HasColumnName(nameof(Customer.Document))
                .HasMaxLength(14);

            document.Property(d => d.Type).HasColumnName(nameof(DocumentType))
                .HasConversion<string>()
                .HasMaxLength(4);
            document.HasIndex(d => d.Number).IsUnique();
        });
    }
}
