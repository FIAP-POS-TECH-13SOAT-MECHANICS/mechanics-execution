using Mechanics.Infra.Security.Models;
using System.Security.Claims;

namespace Mechanics.Infra.Security.Services;

public class CurrentUserService(ClaimsPrincipal user) : ICurrentUserService
{
    private UserData? _userData;

    public UserData GetData() =>
        _userData ??= new UserData
        {
            UserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Role = user.FindFirstValue(ClaimTypes.Role)!,
            CustomerId = Guid.Parse(user.FindFirstValue("customerId")!),
        };
}
