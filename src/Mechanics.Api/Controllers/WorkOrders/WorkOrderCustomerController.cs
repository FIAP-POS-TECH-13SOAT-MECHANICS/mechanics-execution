using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Infra.Security;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.WorkOrders;

/// <summary>
///     Controller para gerenciamento de ordens de serviço.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("work-orders")]
[Authorize(Policy = PolicyNames.CustomersOnly)]
public class WorkOrdersCustomerController(WorkOrderAppService workOrderService, ICurrentUserService currentUserService)
    : ControllerBase
{
    /// <summary>
    ///     Consulta da ordem de serviço pela chave de acesso.
    /// </summary>
    /// <param name="accessKey">Código de acesso de 8 dígitos fornecido ao cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <response code="200">Resultado encontrado.</response>
    /// <response code="404">Não encontrado.</response>
    [HttpGet("track")]
    [Produces("application/json", Type = typeof(GetWorkOrderResponse))]
    [ProducesResponseType(typeof(GetWorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(string accessKey, CancellationToken cancellationToken)
    {
        var customerId = currentUserService.GetData().CustomerId;

        var response = await workOrderService.TrackByAccessKey(customerId, accessKey, cancellationToken);
        return response is not null ? Ok(response) : NotFound();
    }
}
