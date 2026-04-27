using FluentValidation;
using Mechanics.Application.Customers.Requests;

namespace Mechanics.Application.Customers.Validators;

public class CreateIndividualCustomerRequestValidator : AbstractValidator<CreateIndividualCustomerRequest>
{
    public CreateIndividualCustomerRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty();
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.CpfNumber).NotEmpty();
    }
}
