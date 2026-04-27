using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.Property(b => b.Total).HasPrecision(18, 2);
        builder.Property(b => b.ApprovedByCustomerDocument).HasMaxLength(20);
        builder.Property(b => b.Description).HasMaxLength(2000);

        builder.HasOne(b => b.WorkOrder)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Budget)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
