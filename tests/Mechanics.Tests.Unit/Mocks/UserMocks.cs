using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Microsoft.AspNetCore.Identity;

namespace Mechanics.Tests.Unit.Mocks;

public static class UserMocks
{
    private static readonly Dictionary<string, Role> Roles = RoleSeeds.GetSeeds().ToDictionary(role => role.Name);

    public static CreateUserRequest BuildCreateRequest(Guid roleId) =>
        new()
        {
            FullName = "MARIA FERNANDA SOUZA",
            CpfNumber = "12345678909",
            RoleId = roleId,
            Email = "maria.souza@mechanics.com",
        };

    public static UpdateUserRequest BuildUpdateRequest(Guid roleId) =>
        new() { RoleId = roleId };

    public static User CreateUser(Guid userId, string name, string cpf, string roleName)
    {
        var role = Roles[roleName];

        return new User
        {
            Id = userId,
            FullName = name.ToUpper(),
            CpfNumber = cpf,
            RoleId = role.Id,
            Role = role,
            Email = $"{name.Replace(' ', '.').ToLower()}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = userId.ToString(),
        };
    }

    public static User CreateUser(string userName, string userPassword)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = userName.ToUpper(),
            CpfNumber = userName,
            RoleId = Roles[RoleNames.Administrator].Id,
            Role = Roles[RoleNames.Administrator],
            Email = $"{userName}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = userName,
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, userPassword);

        return user;
    }
}
