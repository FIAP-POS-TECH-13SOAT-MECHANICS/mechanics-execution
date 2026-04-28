using Mechanics.Application.Identity.Responses;

namespace Mechanics.Application.Identity.Services;

public interface IUserService
{
    Task<UserResponse?> GetUserById(Guid id, CancellationToken cancellationToken);
}
