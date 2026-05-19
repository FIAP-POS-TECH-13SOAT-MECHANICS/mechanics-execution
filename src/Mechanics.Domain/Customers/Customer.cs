using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.Customers;

public class Customer : AbstractEntity, IValidatable, INormalizable
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required PersonalDocument Document { get; set; }
    public ICollection<Vehicle>? Vehicles { get; init; }

    public void Validate(ValidationBuilder builder) =>
        builder.AddValidation(Name.Length > 0, nameof(Name), "Name is required.")
            .AddValidation(Email.Length > 0, nameof(Email), "Email is required.")
            .AddValidation(Document.Validate, nameof(Document));

    public bool IsNormalized() =>
        Name.IsTrimmedUpperCase() &&
        Email.IsTrimmedLowerCase() &&
        Document.IsNormalized();

    public void Normalize()
    {
        Name = Name.Trim().Normalize().ToUpper();
        Email = Email.Trim().Normalize().ToLower();
        Document.Normalize();
    }
}
