using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Auth.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar cadastros de usuários.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("auth/[controller]")]
[Authorize(Roles = RoleNames.Administrator)]
public class UsersController(UserAppService service) : ControllerBase
{
    /// <summary>
    ///     Cria um novo usuário.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Consumes(typeof(CreateUserRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.Create(request, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Busca usuários por nome.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetUsersResponse))]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Busca um usuário pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetUserResponse))]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Atualiza um usuário.
    /// </summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPut("/{id:guid}")]
    [Consumes(typeof(CreateUserRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }
}
