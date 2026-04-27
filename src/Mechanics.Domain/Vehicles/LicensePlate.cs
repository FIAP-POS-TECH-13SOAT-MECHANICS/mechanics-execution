using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;

namespace Mechanics.Domain.Vehicles;

public class LicensePlate(string number) : IValidatable, INormalizable
{
    public string Number { get; set; } = Normalize(number);

    public static implicit operator string(LicensePlate licensePlate) => licensePlate.Number;
    private static string Normalize(string number) => number.Replace("-", "").ToUpper().Trim();
    public override string ToString() => Number;
    public string Format() => $"{Number[..3]}-{Number[3..]}";

    public void Validate(ValidationBuilder builder) =>
        builder.AddValidation(Number.Length is 7, nameof(Number), "Plate must have 7 characters.")
            .AddValidation(RegexUtils.LicensePlate().IsMatch(Number), nameof(Number), "Plate must have a valid format.");

    public bool IsNormalized() =>
        RegexUtils.LicensePlate().IsMatch(Number);

    public void Normalize() =>
        Number = new string(Number.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    public override bool Equals(object? obj)
    {
        if (obj is not LicensePlate plate)
            return false;

        return plate.Number == Number;
    }

    public override int GetHashCode() => Number.GetHashCode();
}
