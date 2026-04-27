using Mechanics.Application.Products.Requests;
using Mechanics.Application.Products.Responses;
using Mechanics.Application.Products.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Products;

/// <summary>
///     Controller para gerenciar produtos.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("[controller]")]
[Authorize(Policy = PolicyNames.EmployeesOnly)]
public class ProductsController(ProductAppService service) : ControllerBase
{
    /// <summary>
    ///     Busca produtos por nome.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetProductsResponse))]
    [ProducesResponseType(typeof(GetProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Busca um produto pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetProductResponse))]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Cria um novo produto.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateProductRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.Create(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Atualiza um produto.
    /// </summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(UpdateProductRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }

    /// <summary>
    ///     Deleta um produto.
    /// </summary>
    /// <response code="204">Registro deletado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Delete(id, cancellationToken);

        return !response ? NotFound() : NoContent();
    }
}
