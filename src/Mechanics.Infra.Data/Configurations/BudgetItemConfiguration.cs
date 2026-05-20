using Mechanics.Domain.Budgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.Property(i => i.NameSnapshot).HasMaxLength(255);
        builder.Property(i => i.UnitPriceSnapshot).HasPrecision(18, 2);
        builder.Property(i => i.Subtotal).HasPrecision(18, 2);
    }
}
