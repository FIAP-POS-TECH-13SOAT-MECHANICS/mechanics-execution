using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Services;
using Mechanics.Infra.Security;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar informações de login.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("auth")]
[Authorize(Policy = PolicyNames.AllAuthenticated)]
public class AuthController(AuthAppService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    ///     Envia um código de criação de senha para o e-mail do usuário associado ao CPF informado.
    /// </summary>
    /// <remarks>Por segurança é sempre retornado um código de sucesso, mesmo que o CPF informado seja inválido.</remarks>
    /// <response code="204">Resposta padrão.</response>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [Consumes(typeof(ResetPasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(ResetPasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await service.ResetPassword(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Cria uma senha para o usuário associado ao CPF informado.
    /// </summary>
    /// <response code="204">Senha criada com sucesso.</response>
    /// <response code="401">CPF ou código de criação de senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("create-password")]
    [Consumes(typeof(CreatePasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreatePasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePassword(CreatePasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await service.CreatePassword(request, cancellationToken);
        return response is not null ? NoContent() : Unauthorized();
    }

    /// <summary>
    ///     Altera a senha do usuário atual.
    /// </summary>
    /// <response code="204">Senha alterada com sucesso.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpPost("change-password")]
    [Consumes(typeof(ChangePasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(ChangePasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetData().UserId;

        await service.ChangePassword(userId, request, cancellationToken);
        return NoContent();
    }
}
