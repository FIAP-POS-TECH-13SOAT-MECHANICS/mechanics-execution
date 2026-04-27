using System.Text.RegularExpressions;

namespace Mechanics.Domain.Base.Validation;

public static partial class RegexUtils
{
    [GeneratedRegex(@"^\d{11}$", RegexOptions.Compiled)]
    public static partial Regex Cpf();

    [GeneratedRegex(@"^[A-Z\d]{12}\d\d$", RegexOptions.Compiled)]
    public static partial Regex Cnpj();

    [GeneratedRegex(@"^[A-Z]{3}\d[A-Z0-9]\d{2}$", RegexOptions.Compiled)]
    public static partial Regex LicensePlate();

    [GeneratedRegex(@"\p{Mn}", RegexOptions.Compiled)]
    public static partial Regex NonUnicodeChar();

    [GeneratedRegex(@"^\d{4}$", RegexOptions.Compiled)]
    public static partial Regex Year();
}
