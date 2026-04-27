using Mechanics.Domain.Auth;

namespace Mechanics.Infra.Data.Seeds;

public static class RoleSeeds
{
    public static IEnumerable<Role> GetSeeds() =>
    [
        new() { Id = new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), Name = RoleNames.Administrator },
        new() { Id = new Guid("a1097867-aa3e-416c-8685-190516b62a12"), Name = RoleNames.Attendant },
        new() { Id = new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), Name = RoleNames.Mechanic },
        new() { Id = new Guid("f61b4ae9-cc8f-4fda-a39f-f70bb3c0840f"), Name = RoleNames.CustomerUser },
        new() { Id = new Guid("f31bca41-0895-4af5-976f-ac892f833b1b"), Name = RoleNames.CustomerAdmin },
    ];
}
