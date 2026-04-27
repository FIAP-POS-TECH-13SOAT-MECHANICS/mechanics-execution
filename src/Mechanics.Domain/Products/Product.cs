using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Domain.Products;

public class Product : AbstractEntity, IValidatable, INormalizable
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required ProductType Type { get; init; }

    public required int Quantity { get; set; }

    public required ProductStatusType Status { get; set; }
    public IEnumerable<WorkOrderProduct>? WorkOrders { get; init; }
    public decimal UnitPrice { get; init; }

    public void Validate(ValidationBuilder builder) =>
        builder.AddValidation(Quantity >= 0, nameof(Quantity), "Quantity can't be less than 0");

    public bool IsNormalized() => Name.IsTrimmedUpperCase();

    public void Normalize() => Name = Name.Trim().ToUpper();
}
