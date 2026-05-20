using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Identity.Responses;
using Mechanics.Domain.Budgets;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

public static class BudgetEmailTemplates
{
    public static EmailMessage CustomerResponse(UserResponse mechanic, WorkOrder workOrder, Budget budget,
        BudgetRevisedEvent response) => new()
    {
        Recipient = mechanic.Email,
        Subject = response.Approved
            ? $"Orçamento aprovado - Veículo {workOrder.VehicleLicensePlate} - FIAP Mechanics"
            : $"Orçamento rejeitado - Veículo {workOrder.VehicleLicensePlate} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{mechanic.FullName}</b>,</p>
                <p>O orçamento para o veículo <b>{workOrder.VehicleLicensePlate}</b> ({workOrder.Id}) foi aprovado pelo cliente.</p>

                <ul>
                    <li><b>Valor estimado</b>: {budget.Total:C}</li>
                    <li>{(!string.IsNullOrEmpty(response.Notes) ? $"<b>Comentário do cliente</b>: {response.Notes}" : "O cliente não deixou nenhum comentário.")}</li>
                </ul>

                <p>Por favor, inicie a execução assim que possível.</p>

                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };

    public static EmailMessage ExpiredBudget(UserResponse mechanic, WorkOrder wo) => new()
    {
        Recipient = mechanic.Email,
        Subject = $"Orçamento expirado - Veículo {wo.VehicleLicensePlate} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{mechanic.FullName}</b>,</p>
                <p>O orçamento para o veículo <b>{wo.VehicleLicensePlate}</b> ({wo.Id}) expirou.</p>

                <p>O cliente será notificado para que um novo orçamento seja realizado.</p>

                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };

    public static EmailMessage RejectedBudget(UserResponse mechanic, WorkOrder wo, string? notes) => new()
    {
        Recipient = mechanic.Email,
        Subject = $"Orçamento rejeitado - Veículo {wo.VehicleLicensePlate} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{mechanic.FullName}</b>,</p>
                <p>O orçamento para o veículo <b>{wo.VehicleLicensePlate}</b> foi rejeitado pelo cliente.</p>

                <p>{(!string.IsNullOrEmpty(notes) ? $"<b>Comentário do cliente</b>: {notes}" : "O cliente não deixou nenhum comentário.")}</p>

                <p>Por favor, revise o orçamento e envie uma nova proposta.</p>

                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };
}
