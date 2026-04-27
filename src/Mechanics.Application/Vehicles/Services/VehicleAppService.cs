using AutoMapper;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Responses;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Vehicles;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Vehicles.Services;

public class VehicleAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<CreateItemResponse> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Vehicle>(request);

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.Vehicles.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<GetVehicleResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        return vehicle is null ? null : mapper.Map<GetVehicleResponse>(vehicle);
    }

    public async Task<GetVehiclesResponse> GetList(GetVehiclesRequest request, CancellationToken cancellationToken)
    {
        var emptyChassis = string.IsNullOrWhiteSpace(request.Chassis);
        var chassis = request.Chassis?.Trim().ToUpper();
        var emptyPlate = string.IsNullOrWhiteSpace(request.LicensePlate);
        var plate = request.LicensePlate is null ? null : new LicensePlate(request.LicensePlate).Number;
        var emptyOwner = !request.OwnerId.HasValue;

        var query = dbContext.Vehicles
            .Where(v => emptyOwner || v.OwnerId == request.OwnerId!.Value)
            .Where(v => emptyChassis || v.Chassis.Contains(chassis!))
            .Where(v => emptyPlate || v.LicensePlate.Number.Contains(plate!));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetVehiclesResponse(mapper.Map<IEnumerable<GetVehicleResponse>>(items), count);
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Manufacturer = request.Manufacturer ?? entity.Manufacturer;
        entity.Model = request.Model ?? entity.Model;
        entity.Color = request.Color ?? entity.Color;
        entity.Year = request.Year ?? entity.Year;
        entity.Chassis = request.Chassis ?? entity.Chassis;
        entity.OwnerId = request.OwnerId ?? entity.OwnerId;

        if (!string.IsNullOrWhiteSpace(request.LicensePlate))
            entity.LicensePlate = new LicensePlate(request.LicensePlate);

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse { UpdatedItemId = id };
    }
}
