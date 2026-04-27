using Mechanics.Domain.Auth;
using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.WorkOrders;

/// <summary>
///     Representa uma ordem de serviço vinculada a um cliente e veículo.
/// </summary>
public class WorkOrder : AbstractEntity, IValidatable
{
    public required Guid CustomerId { get; init; }
    public Customer? Customer { get; init; }

    /// <summary>
    ///     Chave de acesso para consulta pelo cliente.
    /// </summary>
    public required string AccessKey { get; init; }

    public required Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }

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

    /// <summary>
    ///     Navegação para o usuário atribuído.
    /// </summary>
    public User? AssignedToUser { get; set; }

    /// <summary>
    ///     Gera uma nova chave de acesso única por cliente.
    /// </summary>
    /// <param name="existingOrders">As ordens de serviço do cliente.</param>
    /// <remarks>A chave é composta por 8 dígitos e deve ser única por cliente.</remarks>
    /// <returns>Uma nova chave de acesso para ser usada em <see cref="AccessKey"/>.</returns>
    public static string GenerateNewAccessKey(IEnumerable<WorkOrder> existingOrders)
    {
        var existingKeys = existingOrders.Select(order => order.AccessKey).ToHashSet();

        while (true)
        {
            var newKey = string.Concat(Enumerable.Range(0, 8).Select(_ => Random.Shared.Next(0, 10)));
            if (existingKeys.Add(newKey))
                return newKey;
        }
    }

    public void Validate(ValidationBuilder builder)
    {
        if (Products is null)
            return;

        foreach (var workOrderProduct in Products)
            builder.AddValidation(workOrderProduct.Validate, nameof(Products));
    }
}
