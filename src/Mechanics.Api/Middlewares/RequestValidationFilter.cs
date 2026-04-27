using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mechanics.Api.Middlewares;

public class RequestValidationFilter(ILogger<RequestValidationFilter> logger, IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cancellationToken = context.HttpContext.RequestAborted;

        logger.LogDebug("Validating request for action: {Action}", context.ActionDescriptor.DisplayName);
        var errors = new Dictionary<string, string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            var validationResult = await Validate(argument, cancellationToken);
            if (validationResult is not { IsValid: false })
                continue;

            foreach (var error in validationResult.Errors)
            {
                var camelCasePropertyName = $"{error.PropertyName[0].ToString().ToLower()}{error.PropertyName[1..]}";
                if (errors.TryGetValue(camelCasePropertyName, out var value))
                {
                    var separator = value.EndsWith('.') ? " " : ". ";
                    errors[camelCasePropertyName] = string.Concat(value, separator, error.ErrorMessage);
                    continue;
                }

                errors[camelCasePropertyName] = error.ErrorMessage;
            }
        }

        if (errors.Count == 0)
        {
            await next();
            return;
        }

        logger.LogInformation("Validating failed for action: {Action}", context.ActionDescriptor.DisplayName);

        var response = new ProblemDetails
        {
            Status = 400,
            Extensions = new Dictionary<string, object?> { { "errors", errors } },
        };
        context.Result = new BadRequestObjectResult(response);
    }

    private async Task<ValidationResult?> Validate(object? argument, CancellationToken cancellationToken)
    {
        if (argument is null)
            return null;

        var argumentType = argument.GetType();
        if (argumentType.IsPrimitive || argumentType == typeof(string))
            return null;

        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
        var validator = serviceProvider.GetService(validatorType);
        if (validator is null)
            return null;

        var validateMethod = validatorType.GetMethod("ValidateAsync", [argumentType, cancellationToken.GetType()]);
        if (validateMethod?.Invoke(validator, [argument, cancellationToken]) is not Task<ValidationResult> validationResultTask)
            return null;

        return await validationResultTask;
    }
}
