using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Customers.Requests;

public class GetCustomersRequest : PaginatedListRequest
{
    /// <summary>
    ///     Nome (ou parte do nome) para filtro de busca. Deixe em branco para listar todos.
    /// </summary>
    public string Name { get; init; } = "";
}
