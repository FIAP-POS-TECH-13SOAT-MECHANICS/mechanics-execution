using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Domain.ServicesCatalog;

/// <summary>
///     Entidade que representa um serviço oferecido pela oficina.
/// </summary>
public class ServiceCatalog : AbstractEntity, IValidatable, INormalizable
{
    /// <summary>
    ///     Nome do serviço.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Descrição detalhada do serviço.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    ///     Preço base do serviço.
    /// </summary>
    public required decimal BasePrice { get; set; }

    /// <summary>
    ///     Tempo médio de execução (em minutos).
    /// </summary>
    public required int AverageTime { get; set; }

    /// <summary>
    ///     Status do serviço (Ativo/Inativo).
    /// </summary>
    public required ServiceCatalogStatusType Status { get; set; }

    /// <summary>
    ///     Ordens de serviço associadas a este serviço.
    /// </summary>
    public IEnumerable<WorkOrder>? WorkOrders { get; init; }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddValidation(BasePrice >= 0, nameof(BasePrice), "The base price cannot be negative.");
        builder.AddValidation(AverageTime > 0, nameof(AverageTime), "The average time must be greater than zero.");
    }

    public bool IsNormalized() => Name.IsTrimmedUpperCase();

    public void Normalize() => Name = Name.Trim().ToUpper();
}
