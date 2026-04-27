using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Mechanics.Api.Extensions;

/// <summary>
/// Transforms route tokens like controller/action names from PascalCase to kebab-case.
/// Example: "WeatherForecast" -> "weather-forecast"
/// </summary>
[ExcludeFromCodeCoverage]
public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null)
            return null;

        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str))
            return str;

        var kebab = PascalCase().Replace(str, "-$1");
        return kebab.ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])")]
    private static partial Regex PascalCase();
}
