using System.Diagnostics.Metrics;

namespace Mechanics.Application.Observability;

public static class AppMetrics
{
    public static readonly Meter Meter = new("Mechanics.Api");

    /// <summary>
    /// Volume diário de ordens de serviço.
    /// Dashboard: "Volume diário de OS"
    /// </summary>
    public static readonly Counter<long> WorkOrdersCreated =
        Meter.CreateCounter<long>("work_orders.created");

    /// <summary>
    /// Acumulador do tempo total que cada OS permaneceu no status anterior.
    /// Usado em conjunto com <see cref="TimeInStatusSamples"/> para cálculo manual de média.
    /// </summary>
    public static readonly Counter<double> TimeInStatusTotalSeconds =
        Meter.CreateCounter<double>("work_orders.time_in_status.total_seconds", unit: "s");

    /// <summary>
    /// Contador de amostras para cálculo do tempo médio em cada status.
    /// </summary>
    public static readonly Counter<long> TimeInStatusSamples =
        Meter.CreateCounter<long>("work_orders.time_in_status.samples");

    /// <summary>Total de transições de status de OS.</summary>
    public static readonly Counter<long> StatusTransitions =
        Meter.CreateCounter<long>("work_orders.status_transitions", "transitions",
            "Total de transições de status");

    /// <summary>
    /// Tempo médio de execução por status.
    /// Dashboard: "Tempo médio por status" 
    /// </summary>
    public static readonly Histogram<double> StatusDurationSeconds =
        Meter.CreateHistogram<double>("work_orders.status_duration_seconds", "s",
            "Tempo em segundos que a OS ficou no status anterior");


    /// <summary> 
    /// Integrações de e-mail: total de e-mails enviados e falhas.
    /// </summary>

    public static readonly Counter<long> EmailsSent =
       Meter.CreateCounter<long>("emails.sent", "emails",
           "Total de e-mails enviados com sucesso");

    public static readonly Counter<long> EmailsFailed =
        Meter.CreateCounter<long>("emails.failed", "emails",
            "Total de falhas no envio de e-mails");
}
