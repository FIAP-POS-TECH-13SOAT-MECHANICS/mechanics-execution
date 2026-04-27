using Mechanics.Domain.Base.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Mechanics.Api.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            var routePattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText;
            var httpRoute = routePattern ?? path;

            if (context.Response.HasStarted)
            {
                logger.LogError(e,
                    "Response already started, cannot handle exception | {http.method} {http.route}",
                    method,
                    httpRoute);

                throw;
            }

            await HandleException(context, e, method, httpRoute);
        }
    }

    private async Task HandleException(HttpContext context, Exception exception, string method, string httpRoute)
    {
        var activity = Activity.Current;
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity?.AddException(exception);

        switch (exception)
        {
            case EntityNotFoundException e:
                logger.LogWarning(e,
                    "Entity not found | {HttpMethod} {HttpRoute}",
                    method,
                    httpRoute);

                await WriteProblemDetails(context, StatusCodes.Status400BadRequest, e);
                break;

            case BusinessException e:
                logger.LogWarning(e,
                    "Business rule violation | {HttpMethod} {HttpRoute}",
                    method,
                    httpRoute);

                await WriteProblemDetails(context, StatusCodes.Status400BadRequest, e);
                break;

            default:
                logger.LogError(exception,
                    "Unhandled exception | {HttpMethod} {HttpRoute} | {ExceptionType}: {ExceptionMessage}",
                    method,
                    httpRoute,
                    exception.GetType().Name,
                    exception.Message);

                await WriteProblemDetails(context, StatusCodes.Status500InternalServerError, exception);
                break;
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode, Exception e)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetails
        {
            Status = statusCode,
            Type = e.GetType().FullName,
            Title = GetErrorTitle(statusCode, e),
            Extensions =
            {
                ["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
            },
#if DEBUG
            Detail = JsonSerializer.Serialize(new ExceptionDetails(e), SerializerOptions),
#endif
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }

    private static string GetErrorTitle(int statusCode, Exception exception)
    {
        return statusCode >= 500 ? "Internal server error." : exception.Message;
    }

    public class ExceptionDetails(Exception e)
    {
        public string Name { get; } = e.GetType().Name;
        public string Message { get; } = e.Message;
        public ExceptionDetails? InnerException { get; } = e.InnerException != null ? new ExceptionDetails(e.InnerException) : null;
    }
}
