using FluentValidation;
using Mechanics.Application.Customers.Requests;

namespace Mechanics.Application.Customers.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        When(request => request.Document is not null, () =>
        {
            RuleFor(request => request.Document!.Number).NotEmpty();
            RuleFor(request => request.Document!.Type).IsInEnum();
        });
    }
}
