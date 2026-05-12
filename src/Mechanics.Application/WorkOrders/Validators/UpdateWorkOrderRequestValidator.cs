using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class UpdateWorkOrderRequestValidator : AbstractValidator<UpdateWorkOrderRequest>
{
    public UpdateWorkOrderRequestValidator()
    {
        RuleForEach(request => request.Products)
            .SetValidator(new WorkOrderProductRequestValidator())
            .When(request => request.Products != null && request.Products.Any());
    }
}

public class WorkOrderProductRequestValidator : AbstractValidator<WorkOrderProductRequest>
{
    public WorkOrderProductRequestValidator()
    {
        RuleFor(request => request.ProductId).NotEmpty();
        RuleFor(request => request.Quantity).GreaterThan(0);
    }
}
