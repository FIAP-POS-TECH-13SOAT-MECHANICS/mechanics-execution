using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Application.ServicesCatalog.Responses;

/// <summary>
///     Representa os dados de um serviço oferecido retornado em consultas.
/// </summary>
public class GetServiceCatalogResponse
{
    /// <summary>
    ///     Identificador único do serviço.
    /// </summary>
    public required Guid Id { get; init; }

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
    public required ServiceCatalogStatusType Status { get; init; }
}
