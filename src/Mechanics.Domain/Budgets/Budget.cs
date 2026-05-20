using Mechanics.Domain.Base;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Domain.Budgets;

/// <summary>
///     Representa um orçamento (budget) associado a uma WorkOrder.
/// </summary>
public class Budget : AbstractEntity
{
    public required Guid WorkOrderId { get; init; }
    public WorkOrder? WorkOrder { get; init; }

    /// <summary>
    /// Data de expiração.
    /// </summary>
    /// <remarks>Definimos CreationDate + 3 dias.</remarks>
    public DateTime ExpiresAt => CreationDate.Date.AddDays(3);

    public required BudgetStatus Status { get; set; }

    /// <summary>
    /// Valor total do orçamento.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Quando aprovado pelo cliente.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Anotações do cliente ao aprovar ou rejeitar.
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Itens do orçamento (produtos/serviços com preços no momento do orçamento).
    /// </summary>
    public ICollection<BudgetItem>? Items { get; set; }

    /// <summary>
    /// Quando rejeitado pelo cliente.
    /// </summary>
    public DateTime? RejectedAt { get; set; }
}
