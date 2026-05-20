using Mechanics.Domain.Base;

namespace Mechanics.Domain.Budgets;

/// <summary>
///    Orçamento — snapshot do produto/serviço no momento do orçamento.
/// </summary>
public class BudgetItem : AbstractEntity
{
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }

    public Guid? ProductId { get; set; }
    public Guid? ServiceCatalogId { get; set; }

    public required string NameSnapshot { get; set; }
    public required decimal UnitPriceSnapshot { get; set; }
    public required int Quantity { get; set; }
    public required decimal Subtotal { get; set; }
}
