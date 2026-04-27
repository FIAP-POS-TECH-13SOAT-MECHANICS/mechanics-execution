using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Responses;
using Mechanics.Application.Vehicles.Services;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Vehicles;

/// <summary>
///     Controller para gerenciar cadastros de veículos.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("[controller]")]
[Authorize(Policy = PolicyNames.EmployeesOnly)]
public class VehiclesController(VehicleAppService service) : ControllerBase
{
    /// <summary>
    ///     Cria um novo veículo.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateVehicleRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVehicle(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.Create(request, cancellationToken);
        return CreatedAtAction(nameof(GetVehicle), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Busca veículos pelos filtros informados.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetVehiclesResponse))]
    [ProducesResponseType(typeof(GetVehiclesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetVehicles([FromQuery] GetVehiclesRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Busca um veículo pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetVehicleResponse))]
    [ProducesResponseType(typeof(GetVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicle(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Atualiza um veículo existente.
    /// </summary>
    /// <response code="204">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(UpdateVehicleRequest), "application/json")]
    [Produces("application/json", Type = typeof(UpdateItemResponse))]
    [ProducesResponseType(typeof(UpdateItemResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Id = id;
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }
}
