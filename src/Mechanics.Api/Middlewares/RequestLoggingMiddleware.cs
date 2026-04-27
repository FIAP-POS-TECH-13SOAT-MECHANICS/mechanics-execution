using System.Diagnostics;

namespace Mechanics.Api.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var queryString = context.Request.QueryString.HasValue
            ? context.Request.QueryString.Value
            : null;

        var clientIp =
            context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?.Split(',').FirstOrDefault()?.Trim()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        var userAgent = context.Request.Headers.UserAgent.ToString();
        var scheme = context.Request.Scheme;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText;
            var httpRoute = routePattern ?? path;

            var statusCode = context.Response.StatusCode;
            var durationMs = stopwatch.ElapsedMilliseconds;

            var level =
                statusCode >= 500 ? LogLevel.Error :
                statusCode >= 400 ? LogLevel.Warning :
                LogLevel.Information;

            logger.Log(
                level,
                "HTTP request completed | {http.method} {http.route} {http.status_code} {http.response_duration_ms}ms",
                method,
                httpRoute,
                statusCode,
                durationMs);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "HTTP request details | {http.method} {http.route} {http.status_code} {http.response_duration_ms}ms {http.client_ip} {http.scheme} {http.query_string} {http.user_agent}",
                    method,
                    httpRoute,
                    statusCode,
                    durationMs,
                    clientIp,
                    scheme,
                    queryString,
                    userAgent);
            }
        }
    }
}
