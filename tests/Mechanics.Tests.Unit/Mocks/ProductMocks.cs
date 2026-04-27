using Mechanics.Application.Products.Requests;
using Mechanics.Domain.Products;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public static class ProductMocks
{
    public static CreateProductRequest BuildCreateRequest() => new()
    {
        Name = "Pneu",
        Description = "Pneu Pirelli",
        Type = ProductType.Part,
        Quantity = 10,
        Status = ProductStatusType.Active,
        UnitPrice = 249.99m,
    };

    public static CreateProductRequest BuildInvalidCreateRequest() => new()
    {
        Name = "",
        Description = "",
        Type = ProductType.Part,
        Quantity = -1,
        Status = ProductStatusType.Active,
        UnitPrice = 50,
    };

    public static UpdateProductRequest BuildUpdateRequest() => new()
    {
        Name = "Roda",
        Description = "Roda prateada",
        Quantity = 10,
        Status = ProductStatusType.Active,
    };

    public static UpdateProductRequest BuildInvalidUpdateRequest() => new()
    {
        Name = "",
        Description = "",
        Quantity = -1,
        Status = ProductStatusType.Active,
    };

    public static Product CreateProduct(Guid id) => new()
    {
        Id = id,
        Name = "Roda",
        Description = "Roda preta",
        Type = ProductType.Part,
        Quantity = 10,
        Status = ProductStatusType.Active,
    };

    public static Product CreateInvalidProduct(Guid id) => new()
    {
        Id = id,
        Name = "",
        Description = "",
        Type = ProductType.Part,
        Quantity = -1,
        Status = ProductStatusType.Active,
    };

    public static WorkOrderProduct CreateWorkOrderProduct(Guid? productId = null)
    {
        var id = productId ?? Guid.NewGuid();
        return new WorkOrderProduct
        {
            ProductId = id,
            Product = CreateProduct(id),
            Quantity = 1,
        };
    }
}
