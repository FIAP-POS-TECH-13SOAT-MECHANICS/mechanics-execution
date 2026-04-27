namespace Mechanics.Domain.Base.Validation;

/// <summary>
///     Permite adicionar regras de validação a um objeto.
/// </summary>
public class ValidationBuilder
{
    private readonly Dictionary<string, ICollection<string>> _errors = new();

    /// <summary>
    ///     Adiciona uma regra de validação.
    /// </summary>
    /// <param name="isValid">O resultado da validação.</param>
    /// <param name="fieldName">O nome do campo a ser validado.</param>
    /// <param name="errorMessage">A mensagem a ser exibida se a validação falhar.</param>
    /// <returns>O próprio objeto <see cref="ValidationBuilder"/> para permitir o encadeamento de chamadas.</returns>
    public ValidationBuilder AddValidation(bool isValid, string fieldName, string errorMessage)
    {
        if (isValid)
            return this;

        if (_errors.TryGetValue(fieldName, out var fieldErros))
            fieldErros.Add(errorMessage);
        else
            _errors.Add(fieldName, new List<string> { errorMessage });

        return this;
    }

    /// <summary>
    ///     Adiciona uma regra de validação.
    /// </summary>
    /// <param name="validationAction">A ação de validação que será executada.</param>
    /// <param name="fieldName">O nome do campo associado à validação.</param>
    /// <returns>O próprio objeto <see cref="ValidationBuilder"/> para permitir o encadeamento de chamadas.</returns>
    public ValidationBuilder AddValidation(Action<ValidationBuilder> validationAction, string fieldName)
    {
        var builder = new ValidationBuilder();
        validationAction(builder);
        var result = builder.Build();

        if (result.Valid)
            return this;
        foreach (var (field, errors) in result.Errors)
        {
            var key = $"{fieldName}.{field}";
            if (!_errors.ContainsKey(key))
                _errors.Add(key, []);

            foreach (var error in errors)
                _errors[key].Add(error);
        }

        return this;
    }

    /// <summary>
    ///     Cria um <see cref="ValidationResult"/> com os erros identificados.
    /// </summary>
    public ValidationResult Build() => new() { Errors = _errors };

    public ValidationBuilder AddConditionalValidation(bool condition, Action<ValidationBuilder> conditionalBuilder)
    {
        if (!condition)
            return this;

        conditionalBuilder(this);
        return this;
    }
}

public class ValidationResult
{
    public bool Valid => Errors.Count == 0;
    public required IReadOnlyDictionary<string, ICollection<string>> Errors { get; init; }

    public static ValidationResult Empty => new() { Errors = new Dictionary<string, ICollection<string>>() };

    public void ThrowIfInvalid()
    {
        if (!Valid)
            throw new DomainValidationException(this);
    }
}
