using Mechanics.Domain.Products;

namespace Mechanics.Application.Products.Responses;

public class GetProductResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required ProductType Type { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required ProductStatusType Status { get; init; }
}
