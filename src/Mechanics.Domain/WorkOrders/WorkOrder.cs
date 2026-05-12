using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Domain.WorkOrders;

/// <summary>
///     Representa uma ordem de serviço vinculada a um cliente e veículo.
/// </summary>
public class WorkOrder : AbstractEntity, IValidatable
{
    public required Guid CustomerId { get; init; }

    public required Guid VehicleId { get; init; }

    public WorkOrderStatus Status { get; set; }
    public DateTime LastUpdate { get; set; }

    /// <summary>
    ///     Produtos utilizados na ordem.
    /// </summary>
    public ICollection<WorkOrderProduct>? Products { get; set; }

    /// <summary>
    ///     Serviços executados na ordem.
    /// </summary>
    public ICollection<ServiceCatalog>? ServiceCatalog { get; set; }

    /// <summary>
    ///     Problema relatado pelo cliente.
    /// </summary>
    public string? ReportedProblem { get; init; }

    /// <summary>
    ///     Observações internas da oficina.
    /// </summary>
    public string? Observations { get; set; }

    /// <summary>
    ///     Data em que a ordem foi colocada em aguardando aprovação.
    /// </summary>
    public DateTime? ApprovalRequestedAt { get; set; }

    /// <summary>
    ///     Data em que o cliente aprovou a ordem.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    ///     Data da entrega/retirada do veículo.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    ///     Indica se a ordem foi cancelada.
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    ///     Usuário que realizou a última alteração de status.
    /// </summary>
    public Guid? LastStatusChangeBy { get; set; }

    /// <summary>
    ///     Usuário a quem a OS foi atribuída (mecânico).
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    public void Validate(ValidationBuilder builder)
    {
        if (Products is null)
            return;

        foreach (var workOrderProduct in Products)
            builder.AddValidation(workOrderProduct.Validate, nameof(Products));
    }
}
