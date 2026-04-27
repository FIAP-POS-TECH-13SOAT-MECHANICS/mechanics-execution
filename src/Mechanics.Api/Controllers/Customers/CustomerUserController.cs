using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Auth.Services;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Customers;

/// <summary>
///     Controller para gerenciar usuários de um cliente.
/// </summary>
[ApiController]
[Route("customers/users")]
[Authorize(Roles = RoleNames.CustomerAdmin)]
public class CustomerUserController(UserAppService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    ///     Cria um novo usuário para o cliente autenticado.
    /// </summary>
    /// <response code="201">Usuário criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    [HttpPost]
    [Consumes(typeof(CreateIndividualCustomerRequest), "application/json")]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(CreateIndividualCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = currentUserService.GetData().CustomerId;

        var createUserRequest = new CreateUserForCustomerRequest(customerId, request);
        var response = await service.Create(createUserRequest, cancellationToken);

        return CreatedAtAction(nameof(GetUser), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Busca usuários do cliente autenticado.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetUsersResponse))]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = currentUserService.GetData().CustomerId;
        var response = await service.GetListByCustomerId(customerId, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Busca um usuário específico do cliente autenticado pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetUserResponse))]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var customerId = currentUserService.GetData().CustomerId;
        var response = await service.GetByIdAndCustomerId(customerId, id, cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Atualiza um usuário do cliente autenticado.
    /// </summary>
    /// <response code="204">Registro atualizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPut("{id:guid}")]
    [Consumes(typeof(UpdateUserRequest), "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerId = currentUserService.GetData().CustomerId;
        var response = await service.UpdateByIdAndCustomerId(customerId, id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }
}
