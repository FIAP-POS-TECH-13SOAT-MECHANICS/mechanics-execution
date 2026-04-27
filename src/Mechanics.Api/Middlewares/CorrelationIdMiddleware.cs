using Serilog.Context;

namespace Mechanics.Api.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string Header = "X-Correlation-ID";
    public const string ItemsKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items[ItemsKey] = correlationId;

        using (LogContext.PushProperty("correlation_id", correlationId))
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(Header))
                    context.Response.Headers[Header] = correlationId;

                return Task.CompletedTask;
            });

            await next(context);
        }
    }
 }
