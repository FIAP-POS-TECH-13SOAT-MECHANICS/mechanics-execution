using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.ServicesCatalog.Responses;
using Mechanics.Application.ServicesCatalog.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.ServicesCatalog;

/// <summary>
///     Controller para gerenciar os serviços oferecidos pela oficina.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("[controller]")]
[Authorize(Policy = PolicyNames.EmployeesOnly)]
public class ServiceCatalogController(ServiceCatalogAppService service) : ControllerBase
{
    /// <summary>
    ///     Listar os serviços oferecidos pelos filtros informados.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetServiceCatalogResponse))]
    [ProducesResponseType(typeof(GetServiceCatalogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetServiceCatalog([FromQuery] GetServiceCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Listar os serviços oferecidos pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetServiceCatalogResponse))]
    [ProducesResponseType(typeof(GetServiceCatalogResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Buscar serviços por termo textual.
    /// </summary>
    /// <remarks>Retorna no máximo 10 itens que contenham o termo no nome ou descrição.</remarks>
    /// <response code="200">Resultados encontrados.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet("search")]
    [Produces("application/json", Type = typeof(GetServicesCatalogResponse))]
    [ProducesResponseType(typeof(GetServicesCatalogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSearchServices([FromQuery] string term, CancellationToken cancellationToken = default)
    {
        var search = await service.GetSearch(term, cancellationToken);
        return Ok(search);
    }

    /// <summary>
    ///     Cadastrar um novo serviço oferecido.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateServiceCatalogRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceCatalog(CreateServiceCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.Create(request, cancellationToken);
        return CreatedAtAction(nameof(GetServiceCatalog), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Atualizar um serviço oferecido existente.
    /// </summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(UpdateServiceCatalogRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateServiceCatalog(Guid id, UpdateServiceCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Id = id;
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }

    /// <summary>
    ///     Remover um serviço oferecido.
    /// </summary>
    /// <response code="204">Registro deletado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteServiceCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Delete(id, cancellationToken);

        return !response ? NotFound() : NoContent();
    }
}
