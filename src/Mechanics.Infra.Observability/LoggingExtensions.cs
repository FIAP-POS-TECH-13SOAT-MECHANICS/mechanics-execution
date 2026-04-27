using Mechanics.Application.Options;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

namespace Mechanics.Infra.Observability;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DatadogOptions>(builder.Configuration.GetSection("Datadog"));

        var appInfo = builder.Configuration.GetSection(nameof(AppInfo)).Get<AppInfo>()!;
        var deploymentEnvironment = builder.Environment.EnvironmentName;

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var dataDogOptions = services.GetRequiredService<IOptions<DatadogOptions>>().Value;

            var loggerConfiguration = configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithSpan()
                .Enrich.With(new DatadogTraceEnricher())
                .Enrich.WithProperty("service.name", appInfo.Name)
                .Enrich.WithProperty("service.version", appInfo.Version)
                .Enrich.WithProperty("deployment.environment", deploymentEnvironment)
                .WriteTo.OpenTelemetry(options =>
                {
                    if (!string.IsNullOrEmpty(dataDogOptions.OtlpEndpoint))
                        options.Endpoint = $"{dataDogOptions.OtlpEndpoint}/v1/logs";
                    options.Protocol = OtlpProtocol.HttpProtobuf;

                    options.Headers = new Dictionary<string, string>
                    {
                        ["DD-API-KEY"] = dataDogOptions.ApiKey,
                    };

                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = appInfo.Name,
                        ["service.version"] = appInfo.Version,
                        ["deployment.environment"] = deploymentEnvironment,
                    };
                });

            if (dataDogOptions.UseJsonLogs)
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
            else
                loggerConfiguration.WriteTo.Console();
        });

        return builder;
    }
}
