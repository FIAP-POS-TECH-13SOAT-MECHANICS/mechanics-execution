using Mechanics.Application.Identity.Responses;
using Mechanics.Application.Identity.Services;
using Mechanics.Infra.Security.Models;

namespace Mechanics.Tests.Unit.Mocks;

public class IdentityApiServiceMock : IIdentityApiService
{
    public UserResponse? UserToReturn { get; set; }

    public Task<UserResponse?> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        if (UserToReturn is null)
            return Task.FromResult<UserResponse?>(null);

        if (UserToReturn != null && UserToReturn.Id == id)
            return Task.FromResult<UserResponse?>(UserToReturn);

        return Task.FromResult<UserResponse?>(new UserResponse
        {
            Id = id,
            FullName = "Mock User",
            Email = "mock.user@example.com",
            CpfNumber = "12345678901",
            Role = new RoleResponse
            {
                Id = Guid.NewGuid(),
                Name = RoleNames.Mechanic,
            },
        });
    }
}
