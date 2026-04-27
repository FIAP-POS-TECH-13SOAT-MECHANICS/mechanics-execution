using FluentValidation;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Vehicles.Validators;

public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleRequestValidator(AppDbContext dbContext)
    {
        When(r => r.Color.HasValue, () =>
        {
            RuleFor(r => r.Color!.Value).IsInEnum();
        });

        When(r => r.OwnerId.HasValue, () =>
        {
            RuleFor(r => r.OwnerId!.Value)
                .MustAsync(async (ownerId, ct) => await dbContext.Customers.AnyAsync(c => c.Id == ownerId, ct))
                .WithMessage("Invalid ownerId.");
        });

        When(request => request.LicensePlate is not null, () =>
        {
            RuleFor(r => r.LicensePlate).NotEmpty()
                .MustAsync((req, licensePlate, ct) =>
                    dbContext.Vehicles.AllAsync(c => c.Id == req.Id || c.LicensePlate.Number != licensePlate, ct))
                .WithMessage("License plate must be unique.");
        });

        When(request => request.Chassis is not null, () =>
        {
            RuleFor(r => r.Chassis).NotEmpty()
                .MustAsync((req, chassis, ct) =>
                    dbContext.Vehicles.AllAsync(c => c.Id == req.Id || c.Chassis != chassis, ct))
                .WithMessage("Chassis must be unique.");
        });
    }
}
