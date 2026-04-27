using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Mechanics.Infra.Observability;

public class DatadogTraceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        var activity = Activity.Current;
        if (activity == null) return;

        var traceId = activity.TraceId.ToString();
        var ddTraceId = Convert.ToUInt64(traceId[16..], 16).ToString();
        var ddSpanId = Convert.ToUInt64(activity.SpanId.ToString(), 16).ToString();

        logEvent.AddPropertyIfAbsent(factory.CreateProperty("dd.trace_id", ddTraceId));
        logEvent.AddPropertyIfAbsent(factory.CreateProperty("dd.span_id", ddSpanId));
    }
}
