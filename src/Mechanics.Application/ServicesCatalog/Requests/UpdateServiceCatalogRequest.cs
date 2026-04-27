using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Application.ServicesCatalog.Requests;

/// <summary>
///     Requisição para atualizar parcialmente um serviço oferecido.
/// </summary>
public class UpdateServiceCatalogRequest
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Nome do serviço. Opcional para atualização.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Descrição do serviço. Opcional para atualização.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Preço base do serviço. Opcional para atualização.
    /// </summary>
    public decimal? BasePrice { get; init; }

    /// <summary>
    ///     Tempo médio de execução (em minutos). Opcional para atualização.
    /// </summary>
    public int? AverageTime { get; init; }

    /// <summary>
    ///     Status do serviço. Opcional para atualização.
    /// </summary>
    public ServiceCatalogStatusType? Status { get; init; }
}
