using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Customers.Responses;
using Mechanics.Application.Customers.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Customers;

/// <summary>
///     Controller para gerenciar cadastros de clientes.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("[controller]")]
[Authorize(Policy = PolicyNames.EmployeesOnly)]
public class CustomersController(CustomerAppService service) : ControllerBase
{
    /// <summary>
    ///     Cria um novo cliente.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost("individual")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateIndividualCustomerRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIndividual(CreateIndividualCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.CreateIndividual(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Cria um novo cliente empresarial.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost("business")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(CreateBusinessCustomerRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBusiness(CreateBusinessCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.CreateBusiness(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Busca clientes por nome.
    /// </summary>
    /// <response code="200">Consulta executada.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetCustomersResponse))]
    [ProducesResponseType(typeof(GetCustomersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.GetList(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Busca um cliente pelo ID.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetCustomerResponse))]
    [ProducesResponseType(typeof(GetCustomerResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await service.Get(id, cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    ///     Atualiza um cliente.
    /// </summary>
    /// <response code="200">Registro atualizado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Attendant}")]
    [Consumes(typeof(UpdateCustomerRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await service.Update(id, request, cancellationToken);
        return response is null ? NotFound() : NoContent();
    }
}
