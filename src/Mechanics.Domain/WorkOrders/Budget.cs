using Mechanics.Domain.Base;

namespace Mechanics.Domain.WorkOrders;

/// <summary>
///     Representa um orçamento (budget) associado a uma WorkOrder.
/// </summary>
public class Budget : AbstractEntity
{
    public required Guid WorkOrderId { get; init; }
    public WorkOrder? WorkOrder { get; init; }

    /// <summary>
    /// Data de expiração, 
    /// Definimos CreationDate + 3 dias.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

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
    /// Documento do cliente (CPF/CNPJ) que aprovou.
    /// </summary>
    public string? ApprovedByCustomerDocument { get; set; }

    /// <summary>
    /// Itens do orçamento (produtos/serviços com preços no momento do orçamento).
    /// </summary>
    public ICollection<BudgetItem>? Items { get; set; }

    /// <summary>
    /// Quando rejeitado pelo cliente.
    /// </summary>
    public DateTime? RejectedAt { get; set; }
    public string? Description { get; set; }
}
