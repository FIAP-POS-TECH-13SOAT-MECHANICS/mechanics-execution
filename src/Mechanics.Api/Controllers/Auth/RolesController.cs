using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Auth.Services;
using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar perfis de acesso.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("auth/[controller]")]
[Authorize(Roles = RoleNames.Administrator)]
public class RolesController(RolesAppService service) : ControllerBase
{
    /// <summary>
    ///     Lista os perfis de acesso disponíveis.
    /// </summary>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetRolesResponse))]
    [ProducesResponseType(typeof(GetRolesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var response = await service.GetRoles(cancellationToken);
        return Ok(response);
    }
}
