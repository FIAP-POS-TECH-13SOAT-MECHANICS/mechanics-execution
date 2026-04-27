using Mechanics.Domain.Auth;

namespace Mechanics.Infra.Data.Seeds;

public static class UserSeeds
{
    public static IEnumerable<User> GetSeeds()
    {
        var roles = RoleSeeds.GetSeeds().ToList();

        return
        [
            new User
            {
                Id = new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"),
                FullName = "Administrator User",
                CpfNumber = "12345678909",
                Email = "administrator@mechanics.com",
                RoleId = GetRoleId(RoleNames.Administrator),
                SecurityStamp = "efcaaf76-0535-45fc-a79c-06ab92c064bb",
                PasswordHash = "AQAAAAIAAYagAAAAEPGF9Xsz+ARiCopDgQbQ8gbGubN6bhvNhpKiy8XK2BORE5eV95VywrM9rVE48i2m8w==",
                CreationDate = new DateTime(2025, 10, 12, 12, 0, 0, DateTimeKind.Utc),
            },
            new User
            {
                Id = new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"),
                FullName = "Attendant User",
                CpfNumber = "98765432100",
                Email = "attendant@mechanics.com",
                RoleId = GetRoleId(RoleNames.Attendant),
                SecurityStamp = "370c4d16-8e11-46ca-9004-e1fb9311e49e",
                PasswordHash = "AQAAAAIAAYagAAAAEEo/VptbCYVPiVkoEVHthpWAZUvV/KJ0WJkg+wKbtXJkmMHmSfnpFT4JTLofugBwyQ==",
                CreationDate = new DateTime(2025, 10, 12, 12, 0, 0, DateTimeKind.Utc),
            },
            new User
            {
                Id = new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"),
                FullName = "Mechanic User",
                CpfNumber = "11144477735",
                Email = "mechanic@mechanics.com",
                RoleId = GetRoleId(RoleNames.Mechanic),
                SecurityStamp = "0a3bc211-1220-4d20-80e9-bd850d0dc200",
                PasswordHash = "AQAAAAIAAYagAAAAEKSeHdHtCfN38pakeil4oyEL0d07GBEySe6csY8jmXIKT3oEZVcZR7Jngd9qxFgmkQ==",
                CreationDate = new DateTime(2025, 10, 12, 12, 0, 0, DateTimeKind.Utc),
            },
        ];

        Guid GetRoleId(string roleName) => roles.First(role => role.Name == roleName).Id;
    }
}
