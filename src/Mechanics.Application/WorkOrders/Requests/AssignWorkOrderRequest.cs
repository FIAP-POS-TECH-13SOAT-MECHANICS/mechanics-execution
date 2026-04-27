namespace Mechanics.Application.WorkOrders.Requests;

/// <summary>
///     Request para atribuir uma WorkOrder a um mecânico.
/// </summary>
public class AssignWorkOrderRequest
{  
    public required Guid AssignedToUserId { get; init; }

    /// <summary>
    /// Descrição opcional que será registrado no histórico da ordem.
    /// </summary>
    public string? Description { get; init; }
}
