using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Application.ServicesCatalog.Requests;

/// <summary>
///     Requisição para cadastrar um serviço oferecido pela oficina.
/// </summary>
public class CreateServiceCatalogRequest
{
    /// <summary>
    ///     Nome do serviço.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Descrição detalhada do serviço.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Preço base do serviço.
    /// </summary>
    public required decimal BasePrice { get; init; }

    /// <summary>
    ///     Tempo médio de execução (em minutos).
    /// </summary>
    public required int AverageTime { get; init; }

    /// <summary>
    ///     Status do serviço (Ativo/Inativo).
    /// </summary>
    public ServiceCatalogStatusType? Status { get; init; }
}
