using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using System.Security.Cryptography;
using System.Text;

namespace Mechanics.Domain.Auth;

public class User : AbstractEntity, INormalizable, IValidatable
{
    public required string FullName { get; set; }
    public Role? Role { get; init; }
    public required Guid RoleId { get; set; }
    public required string CpfNumber { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string SecurityStamp { get; set; }
    public Guid? CustomerId { get; init; }
    public Customer? Customer { get; init; }

    public bool IsNormalized() =>
        FullName.IsTrimmedUpperCase() &&
        RegexUtils.Cpf().IsMatch(CpfNumber) &&
        Email.IsTrimmedLowerCase();

    public void Normalize()
    {
        FullName = FullName.ToUpperInvariant().Trim();
        CpfNumber = new string(CpfNumber.Where(char.IsDigit).ToArray());
        Email = Email.ToLowerInvariant().Trim();
    }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddValidation(FullName.Length > 0, nameof(FullName), "Full Name is required.")
            .AddValidation(CpfNumber.Length > 0, nameof(CpfNumber), "CPF Number is required.")
            .AddValidation(DocumentValidations.ValidateCpf(CpfNumber), nameof(CpfNumber), "Invalid CPF.")
            .AddValidation(Email.Length > 0, nameof(Email), "Email is required.");
    }

    public string GetPasswordCreationCode()
    {
        byte[] userData =
        [
            ..Id.ToByteArray(),
            ..Encoding.ASCII.GetBytes(CreationDate.ToString("O")),
            ..Encoding.ASCII.GetBytes(SecurityStamp),
        ];
        var hashBytes = SHA256.HashData(userData);

        return Convert.ToHexString(hashBytes)[..32];
    }
}
