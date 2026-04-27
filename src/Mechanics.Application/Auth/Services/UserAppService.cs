using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base.Validation;
using Mechanics.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Services;

public class UserAppService(AppDbContext dbContext, IMapper mapper, IEmailService emailService) : IAppService
{
    public async Task<CreateItemResponse> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<User>(request);

        entity.Normalize();
        Validator.ValidateAndThrow(entity);

        entity.PasswordHash = new PasswordHasher<User>().HashPassword(entity, Guid.NewGuid().ToString());
        entity.SecurityStamp = Guid.NewGuid().ToString();

        await dbContext.Users.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var passwordCreationCode = entity.GetPasswordCreationCode();
        await emailService.SendUserPasswordCreationCode(entity, passwordCreationCode, cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<GetUserResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null ? null : mapper.Map<GetUserResponse>(user);
    }

    public async Task<GetUsersResponse> GetList(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().ToUpper();
        var emptyName = string.IsNullOrWhiteSpace(request.Name);

        var query = dbContext.Users
            .Include(user => user.Role)
            .Where(user => emptyName || user.FullName.Contains(normalizedName));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetUsersResponse(mapper.Map<IEnumerable<GetUserResponse>>(items), count);
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.FullName = request.FullName ?? entity.FullName;
        entity.RoleId = request.RoleId ?? entity.RoleId;

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse { UpdatedItemId = id };
    }

    public async Task<CreateItemResponse> Create(CreateUserForCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<User>(request);

        entity.Normalize();
        Validator.ValidateAndThrow(entity);

        entity.PasswordHash = new PasswordHasher<User>().HashPassword(entity, Guid.NewGuid().ToString());
        entity.SecurityStamp = Guid.NewGuid().ToString();

        await dbContext.Users.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var passwordCreationCode = entity.GetPasswordCreationCode();
        await emailService.SendCustomerUserPasswordCreationCode(entity, passwordCreationCode, cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<GetUsersResponse> GetListByCustomerId(Guid customerId, GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().ToUpper();
        var emptyName = string.IsNullOrWhiteSpace(request.Name);

        var query = dbContext.Users
            .Include(user => user.Role)
            .Where(user => user.CustomerId == customerId)
            .Where(user => emptyName || user.FullName.Contains(normalizedName));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetUsersResponse(mapper.Map<IEnumerable<GetUserResponse>>(items), count);
    }

    public async Task<GetUserResponse?> GetByIdAndCustomerId(Guid customerId, Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.CustomerId == customerId, cancellationToken);

        return user is null ? null : mapper.Map<GetUserResponse>(user);
    }

    public async Task<UpdateItemResponse?> UpdateByIdAndCustomerId(Guid customerId, Guid id, UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CustomerId == customerId, cancellationToken);

        if (entity is null)
            return null;

        entity.FullName = request.FullName ?? entity.FullName;

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse { UpdatedItemId = id };
    }
}
