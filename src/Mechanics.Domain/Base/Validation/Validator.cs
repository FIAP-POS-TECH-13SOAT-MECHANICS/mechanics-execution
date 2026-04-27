namespace Mechanics.Domain.Base.Validation;

public static class Validator
{
    public static bool Validate(IValidatable validatable)
    {
        var builder = new ValidationBuilder();
        validatable.Validate(builder);

        return builder.Build().Valid;
    }

    public static void ValidateAndThrow(IValidatable validatable)
    {
        var builder = new ValidationBuilder();
        validatable.Validate(builder);
        builder.Build().ThrowIfInvalid();
    }

    public static void BuildAndThrow(Action<ValidationBuilder> validationAction)
    {
        var builder = new ValidationBuilder();
        validationAction(builder);
        builder.Build().ThrowIfInvalid();
    }
}

public class DomainValidationException(ValidationResult validationResult) : Exception("One or more validations failed.")
{
    public ValidationResult ValidationResult { get; } = validationResult;
}
