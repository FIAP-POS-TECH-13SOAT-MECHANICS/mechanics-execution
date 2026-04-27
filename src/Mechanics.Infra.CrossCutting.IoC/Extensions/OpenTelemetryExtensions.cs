using Mechanics.Application.Observability;
using Mechanics.Application.Options;
using Mechanics.Infra.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class OpenTelemetryExtensions
{
    private static readonly ActivitySource ActivitySource = new("Mechanics.Api");

    public static WebApplicationBuilder AddOpenTelemetryObservability(
        this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DatadogOptions>(builder.Configuration.GetSection("Datadog"));

        var appInfo = builder.Configuration.GetSection(nameof(AppInfo)).Get<AppInfo>()!;
        var environment = builder.Environment.EnvironmentName;
        var dataDogOptions = builder.Configuration.GetSection("Datadog").Get<DatadogOptions>()!;
        var otlpEndpoint = dataDogOptions.OtlpEndpoint;
        var samplingRatio = builder.Environment.IsProduction() ? 0.1 : 1.0;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: appInfo.Name,
                    serviceVersion: appInfo.Version)
                .AddAttributes(new Dictionary<string, object> { ["deployment.environment"] = environment }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ActivitySource.Name)
                    .SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(samplingRatio)))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path.Value;
                            return path != null &&
                                   !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.StartsWith($"{appInfo.RoutePrefix}/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) &&
                                   !path.StartsWith($"{appInfo.RoutePrefix}/swagger", StringComparison.OrdinalIgnoreCase);
                        };

                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            var clientIp = request.Headers["X-Forwarded-For"]
                                               .FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim() ??
                                           request.HttpContext.Connection.RemoteIpAddress?.ToString();

                            if (clientIp != null)
                                activity.SetTag("http.client_ip", clientIp);
                        };

                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.Filter = obj =>
                        {
                            if (obj is SqlCommand cmd)
                                return !cmd.CommandText.Contains("__EFMigrationsHistory");

                            return true;
                        };
                    });

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    tracing.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri($"{otlpEndpoint}/v1/traces");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        exporterOptions.TimeoutMilliseconds = 10_000;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(AppMetrics.Meter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                    {
                        exporterOptions.Endpoint = new Uri($"{otlpEndpoint}/v1/metrics");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        metricReaderOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                    });
                }
            });

        return builder;
    }
}
