using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mechanics.Domain.Base.Exceptions;

/// <summary>
///     Exceção para entidades não localizadas durante a execução de uma operação.
/// </summary>
public class EntityNotFoundException : BusinessException
{
    private EntityNotFoundException(string entityName, Guid? key)
        : base(key is null
            ? $"{entityName} not found."
            : $"{entityName} with key '{key}' not found.")
    {
    }

    private EntityNotFoundException(string entityName, string? propertyName, object? propertyValue)
        : base(propertyName is null || propertyValue is null
            ? $"{entityName} not found."
            : $"{entityName} with {propertyName} '{propertyValue}' not found.")
    {
    }

    public static void ThrowIfNull<T>([NotNull] T? entity, Guid? key) where T : AbstractEntity
    {
        if (entity is not null)
            return;

        throw new EntityNotFoundException(typeof(T).Name, key);
    }

    public static void ThrowIfNull<T>([NotNull] T? entity, object propertyValue,
        [CallerArgumentExpression("propertyValue")]
        string propertyName = "") where T : AbstractEntity
    {
        if (entity is not null)
            return;

        throw new EntityNotFoundException(typeof(T).Name, propertyName, propertyValue);
    }

    public static void ThrowIfNotFound<T>([DoesNotReturnIf(false)] bool found, Guid? key) where T : AbstractEntity
    {
        if (found)
            return;

        throw new EntityNotFoundException(typeof(T).Name, key);
    }
}
