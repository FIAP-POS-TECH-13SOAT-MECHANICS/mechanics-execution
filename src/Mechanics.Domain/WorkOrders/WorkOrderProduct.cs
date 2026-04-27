using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Products;

namespace Mechanics.Domain.WorkOrders;

public class WorkOrderProduct : IValidatable
{
    public Guid WorkOrderId { get; init; }
    public WorkOrder? WorkOrder { get; init; }
    public Guid ProductId { get; init; }
    public Product? Product { get; init; }
    public int Quantity { get; set; }

    public void Validate(ValidationBuilder builder) =>
        builder
            .AddValidation(Quantity > 0, nameof(Quantity), "Quantity must be greater than 0");
}
