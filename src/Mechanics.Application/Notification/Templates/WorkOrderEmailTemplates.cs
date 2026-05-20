using Mechanics.Application.Identity.Responses;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

public static class WorkOrderEmailTemplates
{
    public static EmailMessage AssignmentEmail(WorkOrder wo, UserResponse assignedUser, string? comment) => new()
    {
        Recipient = assignedUser.Email,
        Subject = $"Nova Ordem de Serviço Atribuída - Veículo {wo.VehicleLicensePlate} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{assignedUser.FullName}</b>,</p>
                <p>A Ordem de Serviço (<b>{wo.Id}</b>) para o veículo com placa <b>{wo.VehicleLicensePlate}</b> foi atribuída a você.</p>

                {(comment is not null ? $"<p><b>Observação do Gestor</b>: {comment}</p>" : "")}

                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };
}
