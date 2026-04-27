using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    private const int MaxDescriptionLength = 2000;
    public ChangeStatusRequestValidator()
    {
        RuleFor(r => r.NewStatus).IsInEnum().WithMessage("Invalid status value.");

        RuleFor(r => r.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Description must be at most {MaxDescriptionLength} characters.")
            .When(r => !string.IsNullOrWhiteSpace(r.Description));
    }
}
