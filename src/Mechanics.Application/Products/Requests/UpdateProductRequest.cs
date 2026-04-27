using Mechanics.Domain.Products;

namespace Mechanics.Application.Products.Requests;

public class UpdateProductRequest
{
    /// <summary>
    ///     Nome do produto. Opcional para atualização.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Descrição do produto. Opcional para atualização.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Quantidade do produto. Opcional para atualização.
    /// </summary>
    public int? Quantity { get; init; }

    /// <summary>
    ///     Status do produto. Opcional para atualização.
    /// </summary>
    public ProductStatusType? Status { get; init; }

}
