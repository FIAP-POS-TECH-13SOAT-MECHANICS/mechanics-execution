using FluentValidation;
using Mechanics.Application.Products.Requests;

namespace Mechanics.Application.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Description).NotEmpty();
        RuleFor(request => request.Type).IsInEnum();
        RuleFor(request => request.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(request => request.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
