using AutoMapper;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.ServicesCatalog.Responses;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.ServicesCatalog.Services;

public class ServiceCatalogAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<GetServicesCatalogResponse> GetList(GetServiceCatalogRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.ServiceCatalog.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim().ToUpperInvariant();
            query = query.Where(c => c.Name.Contains(normalizedName));
        }

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status);

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetServicesCatalogResponse(mapper.Map<IEnumerable<GetServiceCatalogResponse>>(items), count);
    }

    public async Task<GetServiceCatalogResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ServiceCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return entity is null ? null : mapper.Map<GetServiceCatalogResponse>(entity);
    }

    public async Task<GetServicesCatalogResponse> GetSearch(string term, CancellationToken cancellationToken)
    {
        var normalizedTerm = term.Trim().ToUpper();

        var query = dbContext.ServiceCatalog
            .AsNoTracking()
            .Where(s =>
                s.Name.Contains(normalizedTerm) ||
                EF.Functions.Like(s.Description, $"%{normalizedTerm}%"))
            .OrderBy(s => s.Name)
            .Take(10); 

        var items = await query.ToListAsync(cancellationToken);
        return new GetServicesCatalogResponse(mapper.Map<IEnumerable<GetServiceCatalogResponse>>(items), items.Count);
    }


    public async Task<CreateItemResponse> Create(CreateServiceCatalogRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<ServiceCatalog>(request);

        if (!entity.IsNormalized())
            entity.Normalize();
        Validator.ValidateAndThrow(entity);

        await dbContext.ServiceCatalog.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateServiceCatalogRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ServiceCatalog
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Name = request.Name?.Trim().ToUpper() ?? entity.Name;
        entity.Description = request.Description ?? entity.Description;
        entity.BasePrice = request.BasePrice ?? entity.BasePrice;
        entity.AverageTime = request.AverageTime ?? entity.AverageTime;
        entity.Status = request.Status ?? entity.Status;

        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse { UpdatedItemId = id };
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ServiceCatalog
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return false;

        Validator.ValidateAndThrow(entity);

        dbContext.ServiceCatalog.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
