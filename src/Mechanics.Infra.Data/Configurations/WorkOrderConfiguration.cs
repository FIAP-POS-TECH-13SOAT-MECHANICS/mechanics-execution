using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.Property(entity => entity.Status);

        builder.Property(entity => entity.LastUpdate)
            .HasDefaultValueSql("SYSDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(entity => entity.AccessKey).HasMaxLength(8).IsFixedLength();
        builder.HasIndex(entity => new { entity.CustomerId, entity.AccessKey }).IsUnique();

        builder.HasOne(entity => entity.Customer).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Vehicle).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(entity => entity.ServiceCatalog)
            .WithMany(entity => entity.WorkOrders);

        builder.Property(e => e.ReportedProblem).HasMaxLength(1000);
        builder.Property(e => e.Observations).HasMaxLength(2000);

        builder.HasOne(e => e.AssignedToUser).WithMany();
        builder.HasIndex(e => e.AssignedToUserId);
    }
}
