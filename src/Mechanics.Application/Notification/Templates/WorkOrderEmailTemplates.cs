using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

public static class WorkOrderEmailTemplates
{
    public static EmailMessage WorkOrderCreated(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = "Ordem de serviço criada - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Uma nova ordem de serviço foi criada para você:</p>

                <ul>
                <li><b>Veículo</b>: {workOrder.Vehicle}</li>
                <li><b>Data de criação</b>: {workOrder.CreationDate:G}</li>
                <li><b>Problema relatado</b>: {workOrder.ReportedProblem ?? "—"}</li>
                </ul>

                <p>Você pode consultar o andamento do serviço acessando o sistema e informando o código abaixo:<br/>
                <code style="font-weight: bold;">{workOrder.AccessKey[..4]} {workOrder.AccessKey[4..]}</code></p>
                """,
    };

    public static EmailMessage WorkOrderStatusChanged(Customer customer, WorkOrder workOrder, WorkOrderStatus previousStatus)
    {
        return new EmailMessage
        {
            Recipient = customer.Email,
            Subject = $"Atualização da OS {workOrder.AccessKey} - {Translate(workOrder.Status)} - FIAP Mechanics",
            Body = $"""
                    <p>Olá, <b>{customer.Name}</b>,</p>
                    <p>Sua ordem de serviço ({workOrder.AccessKey}) mudou de status:</p>
                    <p><b>{Translate(previousStatus)}</b> → <b>{Translate(workOrder.Status)}</b></p>
                    <p>Data: {DateTime.Now:G}</p>
                    """,
        };

        string Translate(WorkOrderStatus status) => status switch
        {
            WorkOrderStatus.Received => "Recebida",
            WorkOrderStatus.UnderDiagnosis => "Em diagnóstico",
            WorkOrderStatus.PendingApproval => "Aguardando aprovação",
            WorkOrderStatus.InProgress => "Em execução",
            WorkOrderStatus.Completed => "Finalizada",
            WorkOrderStatus.Delivered => "Entregue",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    public static EmailMessage WorkOrderCancelled(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = $"OS {workOrder.AccessKey} cancelada - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Sua ordem de serviço ({workOrder.AccessKey}) foi cancelada.</p>
                """,
    };

    public static EmailMessage WorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = $"Pesquisa de satisfação - Ordem {workOrder.AccessKey}",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Seu veículo foi entregue.</p>
                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };
}
