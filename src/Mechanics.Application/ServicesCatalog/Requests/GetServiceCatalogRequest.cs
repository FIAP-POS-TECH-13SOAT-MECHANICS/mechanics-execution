using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Application.ServicesCatalog.Requests;

/// <summary>
///     Requisição para consulta paginada de serviços oferecidos.
/// </summary>
public class GetServiceCatalogRequest : PaginatedListRequest
{
    /// <summary>
    ///     Nome (ou parte do nome) para filtro de busca. Deixe em branco para listar todos.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    ///     Status do serviço (Ativo/Inativo).
    /// </summary>
    public ServiceCatalogStatusType? Status { get; init; }
}
