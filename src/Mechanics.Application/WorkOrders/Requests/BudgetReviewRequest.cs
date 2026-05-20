namespace Mechanics.Application.WorkOrders.Requests;

/// <summary>
///     Requisição para aprovação de budget pelo cliente.
/// </summary>
public abstract class BudgetReviewRequest
{
    public required string AccessKey { get; init; }

    /// <summary>
    /// Comentário opcional informado pelo cliente (para aprovar ou rejeitar).
    /// </summary>
    public string? Description { get; init; }
}
