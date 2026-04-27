using System.ComponentModel.DataAnnotations;

namespace Mechanics.Application.Utils.PagedList;

public class PaginatedListRequest
{
    /// <summary>
    ///     Número da página, iniciando em 1.
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
    public required int Page { get; init; } = 1;

    /// <summary>
    ///     Quantidade de itens por página.
    /// </summary>
    /// <example>10</example>
    [Range(1, int.MaxValue, ErrorMessage = "Items per page must be greater than 0.")]
    public required int ItemsPerPage { get; init; } = 10;
}
