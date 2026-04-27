using FluentValidation;
using Mechanics.Application.Auth.Requests;

namespace Mechanics.Application.Auth.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(request => request.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .Length(8, 64).WithMessage("Password must be between 8 and 64 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
