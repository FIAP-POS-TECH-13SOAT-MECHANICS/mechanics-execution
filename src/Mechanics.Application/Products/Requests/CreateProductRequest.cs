using Mechanics.Domain.Products;

namespace Mechanics.Application.Products.Requests;

public class CreateProductRequest
{
    /// <summary>
    ///     Nome do produto.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Descrição do produto
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Tipo do produto.
    /// </summary>
    public required ProductType Type { get; init; }

    /// <summary>
    ///     Quantidade do produto.
    /// </summary>
    public required int Quantity { get; init; }

    /// <summary>
    ///     Preço unitário do produto.
    /// </summary>
    public required decimal UnitPrice { get; init; }

    /// <summary>
    ///     Status do produto. Opcional para atualização.
    /// </summary>
    public ProductStatusType? Status { get; init; }
}
