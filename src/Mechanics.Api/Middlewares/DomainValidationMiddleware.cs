using Mechanics.Domain.Base.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Mechanics.Api.Middlewares;

public class DomainValidationMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainValidationException e)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var errors = e.ValidationResult.Errors
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var response = new ProblemDetails
            {
                Status = 400,
                Extensions = new Dictionary<string, object?> { { "errors", errors } },
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }
    }
}
