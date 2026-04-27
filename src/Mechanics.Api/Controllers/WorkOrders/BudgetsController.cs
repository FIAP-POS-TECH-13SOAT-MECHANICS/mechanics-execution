using Mechanics.Application.WorkOrders.Requests;
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
public class BudgetsController(BudgetAppService budgetService, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    ///     Aprova um orçamento associado à ordem de serviço.
    /// </summary>
    /// <param name="request">Documento e accessKey do cliente.</param>
    /// <param name="cancellationToken">Token para cancelamento.</param>
    [HttpPost("approve-budget")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBudget(BudgetReviewRequest request, CancellationToken cancellationToken)
    {
        var customerId = currentUserService.GetData().CustomerId;
        await budgetService.ApproveBudget(customerId, request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Rejeita um orçamento associado à ordem de serviço.
    /// </summary>
    [HttpPost("reject-budget")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectBudget(BudgetReviewRequest request, CancellationToken cancellationToken)
    {
        var customerId = currentUserService.GetData().CustomerId;
        await budgetService.RejectBudget(customerId, request.AccessKey, request.Description, cancellationToken);
        return NoContent();
    }
}
