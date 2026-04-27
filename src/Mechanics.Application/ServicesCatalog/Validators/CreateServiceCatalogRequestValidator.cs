using FluentValidation;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.ServicesCatalog.Validators;

/// <summary>
///     Validador para criação de serviços no catálogo.
/// </summary>
public class CreateServiceCatalogRequestValidator : AbstractValidator<CreateServiceCatalogRequest>
{
    public CreateServiceCatalogRequestValidator(AppDbContext dbContext)
    {
        RuleFor(r => r.Name).NotEmpty()
            .MaximumLength(100)
            .MustAsync((serviceName, cancellationToken) =>
                dbContext.ServiceCatalog.AllAsync(s => s.Name != serviceName, cancellationToken))
            .WithMessage("Service name must be unique.");

        RuleFor(r => r.Description).NotEmpty().MaximumLength(255);

        RuleFor(r => r.BasePrice).GreaterThanOrEqualTo(0);

        RuleFor(r => r.AverageTime).GreaterThan(0);

        RuleFor(r => r.Status).IsInEnum();
    }
}
