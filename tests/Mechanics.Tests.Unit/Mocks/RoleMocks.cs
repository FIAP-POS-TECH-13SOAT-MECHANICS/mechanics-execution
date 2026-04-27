using Mechanics.Domain.Auth;
using System.Reflection;

namespace Mechanics.Tests.Unit.Mocks;

public static class RoleMocks
{
    /// <summary>
    ///     Retorna a lista de nomes de roles presentes no projeto.
    /// </summary>
    public static IReadOnlyList<Role> GetRoleNames() =>
        typeof(RoleNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null)!)
            .Select(name => new Role { Id = Guid.NewGuid(), Name = name })
            .ToList();
}
