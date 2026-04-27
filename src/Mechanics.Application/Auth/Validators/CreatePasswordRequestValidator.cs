using FluentValidation;
using Mechanics.Application.Auth.Requests;

namespace Mechanics.Application.Auth.Validators;

public class CreatePasswordRequestValidator : AbstractValidator<CreatePasswordRequest>
{
    public CreatePasswordRequestValidator()
    {
        RuleFor(request => request.CpfNumber)
            .NotEmpty().WithMessage("CPF number is required.");

        RuleFor(request => request.PasswordCreationCode)
            .NotEmpty().WithMessage("Password creation code is required.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(8, 64).WithMessage("Password must be between 8 and 64 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
