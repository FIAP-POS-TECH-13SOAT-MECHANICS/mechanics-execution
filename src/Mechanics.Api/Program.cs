using Mechanics.Api.Extensions;
using Mechanics.Api.Middlewares;
using Mechanics.Application.Options;
using Mechanics.Infra.CrossCutting.IoC.Extensions;
using Mechanics.Infra.Observability;
using Mechanics.Infra.Security.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mechanics.Api;

public class Program
{
    [ExcludeFromCodeCoverage]
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddStructuredLogging();
        builder.AddOpenTelemetryObservability();

        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
            options.Filters.Add<RequestValidationFilter>();
        }).AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerDocumentation(builder.Configuration);

        builder.Services.AddDbContext(builder.Configuration)
            .AddJwtAuthentication(validateInDebugMode: false)
            .AddAppServices(builder.Configuration)
            .AddRequestValidators()
            .AddMessaging(builder.Configuration)
            .AddEmailSender(builder.Configuration);

        builder.Services.AddHealthChecks()
            .AddDbHealthCheck();

        builder.Services.AddGlobalCorsPolicy();

        var app = builder.Build();
        var appInfo = builder.Configuration.GetSection(nameof(AppInfo)).Get<AppInfo>()!;
        app.UsePathBase($"/{appInfo.RoutePrefix}");

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<DomainValidationMiddleware>();

        app.UseRouting();
        app.UseCors("AllowAllOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers()
            .RequireAuthorization();

        if (app.Environment.IsDevelopment())
            await app.ApplyMigrations();

        if (!app.Environment.IsProduction())
            app.UseSwaggerDocumentation($"/{appInfo.RoutePrefix}");

        app.UseHealthChecks("/health");

        await app.RunAsync();
    }
}
