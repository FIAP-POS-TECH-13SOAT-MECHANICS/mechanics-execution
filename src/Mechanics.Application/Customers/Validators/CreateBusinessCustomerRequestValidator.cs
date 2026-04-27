using FluentValidation;
using Mechanics.Application.Customers.Requests;

namespace Mechanics.Application.Customers.Validators;

public class CreateBusinessCustomerRequestValidator : AbstractValidator<CreateBusinessCustomerRequest>
{
    public CreateBusinessCustomerRequestValidator()
    {
        RuleFor(request => request.CompanyName).NotEmpty();
        RuleFor(request => request.CnpjNumber).NotEmpty();
        RuleFor(request => request.ResponsibleFullName).NotEmpty();
        RuleFor(request => request.ResponsibleEmail).NotEmpty().EmailAddress();
        RuleFor(request => request.ResponsibleCpfNumber).NotEmpty();
    }
}
