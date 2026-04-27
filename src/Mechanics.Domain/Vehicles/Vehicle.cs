using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;

namespace Mechanics.Domain.Vehicles;

public class Vehicle : AbstractEntity, IValidatable, INormalizable
{
    public required string Manufacturer { get; set; }
    public required string Model { get; set; }
    public required VehicleColor Color { get; set; }
    public required string Year { get; set; }
    public required LicensePlate LicensePlate { get; set; }
    public required string Chassis { get; set; }
    public Customer? Owner { get; init; }
    public required Guid OwnerId { get; set; }

    public void Validate(ValidationBuilder builder) =>
        builder.AddValidation(Year.Length is 4, nameof(Year), "Year must have 4 digits.")
            .AddValidation(LicensePlate.Validate, nameof(LicensePlate));

    public bool IsNormalized() =>
        Manufacturer.IsTrimmedUpperCase() &&
        Model.IsTrimmedUpperCase() &&
        RegexUtils.Year().IsMatch(Year) &&
        LicensePlate.IsNormalized();

    public void Normalize()
    {
        Manufacturer = Manufacturer.Trim().Normalize().ToUpper();
        Model = Model.Trim().Normalize().ToUpper();
        Year = new string(Year.Where(char.IsDigit).ToArray());
        LicensePlate.Normalize();
    }

    public override string ToString() => $"{Manufacturer} {Model} {Year} - {LicensePlate.Format()}";
}
