using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Products.Requests;

public class GetProductsRequest : PaginatedListRequest
{
    /// <summary>
    ///     Nome (ou parte do nome) para filtro de busca. Deixe em branco para listar todos.
    /// </summary>
    public string Name { get; init; } = "";
}
