using FluentValidation;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.ServicesCatalog.Validators;

public class UpdateServiceCatalogRequestValidator : AbstractValidator<UpdateServiceCatalogRequest>
{
    public UpdateServiceCatalogRequestValidator(AppDbContext dbContext)
    {
        When(request => request.Name is not null, () =>
        {
            RuleFor(r => r.Name)
                .MaximumLength(100)
                .MustAsync((req, serviceName, cancellationToken) =>
                    dbContext.ServiceCatalog.AllAsync(s => s.Id == req.Id || s.Name != serviceName, cancellationToken))
                .WithMessage("Service name must be unique.");
        });

        When(request => request.Description is not null,
            () => RuleFor(r => r.Description).NotEmpty().MaximumLength(255));
        When(request => request.BasePrice is not null,
            () => RuleFor(r => r.BasePrice).GreaterThanOrEqualTo(0));
        When(request => request.AverageTime is not null,
            () => RuleFor(r => r.AverageTime).GreaterThan(0));
    }
}
