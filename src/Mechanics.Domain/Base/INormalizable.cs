using System.Diagnostics.Contracts;

namespace Mechanics.Domain.Base;

public interface INormalizable
{
    /// <summary>
    ///     Indica que o objeto está normalizado, de acordo com <see cref="Normalize()"/>.
    /// </summary>
    /// <returns><c>true</c> se o objeto estiver normalizado, caso contrário, <c>false</c>.</returns>
    [Pure]
    bool IsNormalized();

    /// <summary>
    ///     Normaliza o objeto.
    /// </summary>
    void Normalize();
}

public static class NormalizableExtensions
{
    public static bool IsTrimmedUpperCase(this string str) =>
        !str.StartsWith(' ') &&
        !str.EndsWith(' ') &&
        str.IsNormalized() &&
        str.All(c => !char.IsLetter(c) || char.IsUpper(c));

    public static bool IsTrimmedLowerCase(this string str) =>
        !str.StartsWith(' ') &&
        !str.EndsWith(' ') &&
        str.IsNormalized() &&
        str.All(c => !char.IsLetter(c) || char.IsLower(c));
}
