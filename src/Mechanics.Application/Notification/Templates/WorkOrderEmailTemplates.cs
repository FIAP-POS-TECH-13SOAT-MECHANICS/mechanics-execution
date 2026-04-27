using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using System.Text;

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

    public static EmailMessage WorkOrderPendingApproval(Customer customer, WorkOrder workOrder, Budget budget) => new()
    {
        Recipient = customer.Email,
        Subject = "Orçamento da OS disponível - FIAP Mechanics",
        Body = BuildPendingApprovalBody(customer, workOrder, budget),
    };

    private static string BuildPendingApprovalBody(Customer customer, WorkOrder workOrder, Budget budget)
    {
        var sb = new StringBuilder();

        sb.Append($"""
                   <p>Olá, <b>{customer.Name}</b>,</p>
                   <p>O orçamento da sua ordem de serviço está pronto e aguarda sua aprovação.</p>
                   <ul>
                       <li><b>Ordem</b>: {workOrder.AccessKey}</li>
                       <li><b>Orçamento</b>: {budget.Id}</li>
                       <li><b>Valor estimado</b>: {budget.Total:C}</li>
                   </ul>
                   """);

        sb.Append("<p>Resumo dos itens:</p><ul>");
        if (budget.Items != null && budget.Items.Count != 0)
            foreach (var item in budget.Items)
                sb.Append($"<li>{item.NameSnapshot} — {item.Quantity}x {item.UnitPriceSnapshot:C} = {item.Subtotal:C}</li>");
        else
            sb.Append("<li>— Nenhum item listado —</li>");
        sb.Append("</ul>");

        sb.Append("<p>Para aprovar ou rejeitar o orçamento, acesse nosso sistema.</p><br />");

        sb.Append($"<p>O orçamento expira em: {budget.ExpiresAt?.ToString("t") ?? "—"}</p>");
        sb.Append("<p>Obrigado,<br/>FIAP Mechanics</p>");

        return sb.ToString();
    }

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

    public static EmailMessage MechanicBudgetDecision(User mechanic, WorkOrder workOrder, Budget budget, bool approved) => new()
    {
        Recipient = mechanic.Email,
        Subject = approved
            ? $"Orçamento aprovado - OS {workOrder.AccessKey} - FIAP Mechanics"
            : $"Orçamento rejeitado - OS {workOrder.AccessKey} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{mechanic.FullName}</b>,</p>
                <p>O orçamento da ordem <b>{workOrder.AccessKey}</b> ({workOrder.Id}) foi {(approved ? "aprovado" : "rejeitado")} pelo cliente.</p>

                <ul>
                    <li><b>Valor estimado</b>: {budget.Total:C}</li>
                    <li>{(budget.Description is not null ? $"<b>Comentário do cliente</b>: {budget.Description}" : "O cliente não deixou nenhum comentário.")}</li>
                </ul>

                <p>{(approved ? "Por favor, inicie a execução quando apropriado." : "Por favor, revise o orçamento e proceda com ajustes necessários.")}</p>

                <p>Obrigado,<br/>FIAP Mechanics</p>
                """,
    };
}
