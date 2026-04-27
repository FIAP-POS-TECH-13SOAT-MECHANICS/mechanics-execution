using FluentValidation;
using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Base.Validation;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(AppDbContext dbContext)
    {
        RuleFor(request => request.RoleId).NotEmpty()
            .MustAsync((roleId, cancellationToken) => dbContext.Roles.AnyAsync(r => r.Id == roleId, cancellationToken));

        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);

        RuleFor(request => request.CpfNumber).NotEmpty()
            .Must(cpf => DocumentValidations.ValidateCpf(new string(cpf.Where(char.IsDigit).ToArray())));

        RuleFor(request => request.CpfNumber)
            .MustAsync((cpf, cancellationToken) => dbContext.Users.AllAsync(u => u.CpfNumber != cpf, cancellationToken));

        RuleFor(request => request.Email).NotEmpty().EmailAddress()
            .MustAsync(async (cpf, cancellationToken) =>
            {
                var normalizedCpf = new string(cpf.Where(char.IsDigit).ToArray());
                return !await dbContext.Users.AnyAsync(u => u.CpfNumber == normalizedCpf, cancellationToken);
            });
    }
}
