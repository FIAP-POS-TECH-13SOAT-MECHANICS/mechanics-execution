using FluentValidation;
using Mechanics.Application.Auth.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(AppDbContext dbContext)
    {
        When(request => request.RoleId.HasValue, () =>
        {
            RuleFor(request => request.RoleId)
                .MustAsync((roleId, cancellationToken) => dbContext.Roles.AnyAsync(r => r.Id == roleId, cancellationToken))
                .WithMessage("Invalid roleId.");
        });

        When(request => !string.IsNullOrEmpty(request.FullName), () =>
        {
            RuleFor(request => request.FullName).MaximumLength(100);
        });
    }
}
