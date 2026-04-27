using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class WorkOrderHistoryConfiguration : IEntityTypeConfiguration<WorkOrderHistory>
{
    public void Configure(EntityTypeBuilder<WorkOrderHistory> builder)
    {
        builder.HasOne(h => h.WorkOrder).WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(h => h.Action).HasMaxLength(100);

        builder.Property(h => h.Details).HasMaxLength(2000);
    }
}
