using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mechanics.Infra.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(entity => entity.FullName).HasMaxLength(100);
        builder.HasIndex(entity => entity.FullName);

        builder.Property(entity => entity.CpfNumber).HasMaxLength(11);
        builder.HasIndex(entity => entity.CpfNumber).IsUnique();

        builder.Property(entity => entity.Email).HasMaxLength(100);
        builder.HasIndex(entity => entity.Email).IsUnique();

        builder.Property(entity => entity.PasswordHash).HasMaxLength(256);

        builder.Property(entity => entity.SecurityStamp).HasMaxLength(64);

        builder.HasOne(entity => entity.Role).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Customer).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasData(UserSeeds.GetSeeds());
    }
}
