using AutoMapper;
using Mechanics.Application.Products.Requests;
using Mechanics.Application.Products.Responses;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Products;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Products.Services;

public class ProductAppService(AppDbContext dbContext, IMapper mapper) : IAppService
{
    public async Task<GetProductsResponse> GetList(GetProductsRequest request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim().ToUpper();
        var emptyName = string.IsNullOrWhiteSpace(request.Name);

        var query = dbContext.Products
            .Where(c => emptyName || c.Name.Contains(normalizedName));

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);
        return new GetProductsResponse(mapper.Map<IEnumerable<GetProductResponse>>(items), count);
    }

    public async Task<GetProductResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return product is null ? null : mapper.Map<GetProductResponse>(product);
    }

    public async Task<CreateItemResponse> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Product>(request);

        Validator.ValidateAndThrow(entity);

        await dbContext.Products.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateItemResponse { CreatedId = entity.Id };
    }

    public async Task<UpdateItemResponse?> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Products
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Name = request.Name ?? entity.Name;
        entity.Description = request.Description ?? entity.Description;
        entity.Quantity = request.Quantity ?? entity.Quantity;
        entity.Status = request.Status ?? entity.Status;

        Validator.ValidateAndThrow(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateItemResponse { UpdatedItemId = id };
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Products
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            return false;

        Validator.ValidateAndThrow(entity);

        dbContext.Products.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
