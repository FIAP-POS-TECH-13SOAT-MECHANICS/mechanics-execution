using Mechanics.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class WorkOrderProductConfiguration : IEntityTypeConfiguration<WorkOrderProduct>
{
    public void Configure(EntityTypeBuilder<WorkOrderProduct> builder)
    {
        builder.HasKey(entity => new { entity.WorkOrderId, entity.ProductId });

        builder.HasOne(entity => entity.WorkOrder)
            .WithMany(entity => entity.Products);

        builder.HasOne(entity => entity.Product)
            .WithMany(entity => entity.WorkOrders);
    }
}
