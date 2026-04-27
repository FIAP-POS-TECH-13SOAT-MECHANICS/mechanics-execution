using AutoMapper;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Utils;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Services;

public class RolesAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<GetRolesResponse> GetRoles(CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetRolesResponse(mapper.Map<IEnumerable<RoleResponse>>(roles));
    }
}
