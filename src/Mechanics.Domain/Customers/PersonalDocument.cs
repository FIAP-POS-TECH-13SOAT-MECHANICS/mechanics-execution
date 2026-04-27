using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;

namespace Mechanics.Domain.Customers;

public class PersonalDocument(DocumentType type, string number) : INormalizable, IValidatable
{
    public DocumentType Type { get; } = type;
    public string Number { get; private set; } = number;

    public static implicit operator string(PersonalDocument document) => document.Number;

    public bool IsNormalized() =>
        Type == DocumentType.Cpf && RegexUtils.Cpf().IsMatch(Number) ||
        Type == DocumentType.Cnpj && RegexUtils.Cnpj().IsMatch(Number);

    public void Normalize()
    {
        Number = Type == DocumentType.Cpf
            ? new string(Number.Where(char.IsDigit).ToArray())
            : new string(Number.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddConditionalValidation(Type == DocumentType.Cpf, conditionalBuilder =>
            conditionalBuilder.AddValidation(DocumentValidations.ValidateCpf(Number), nameof(Number), "Invalid CPF."));
        builder.AddConditionalValidation(Type == DocumentType.Cnpj, conditionalBuilder =>
            conditionalBuilder.AddValidation(DocumentValidations.ValidateCnpj(Number), nameof(Number), "Invalid CNPJ."));
    }

    public override string ToString() => Number;

    public override bool Equals(object? obj)
    {
        if (obj is not PersonalDocument document)
            return false;

        return document.Number == Number;
    }

    public override int GetHashCode() => Number.GetHashCode();
}

public enum DocumentType
{
    Cpf,
    Cnpj,
}
