using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(entity => entity.Name).HasMaxLength(50);
        builder.HasIndex(entity => entity.Name).IsUnique();

        builder.HasData(RoleSeeds.GetSeeds());
    }
}
