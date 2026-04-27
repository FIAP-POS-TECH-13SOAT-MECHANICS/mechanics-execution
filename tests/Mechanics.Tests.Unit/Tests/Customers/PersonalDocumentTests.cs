using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;

namespace Mechanics.Tests.Unit.Tests.Customers;

[TestClass]
[TestCategory("Customers")]
[TestCategory("PersonalDocuments")]
public class PersonalDocumentTests
{
    #region Normalizações

    [TestMethod("Verifica normalização de CPF")]
    public void It_ShouldNormalizeCpf()
    {
        var document = new PersonalDocument(DocumentType.Cpf, "935.568.660-99");

        if (!document.IsNormalized())
            document.Normalize();

        Assert.AreEqual("93556866099", document.Number);
        Assert.IsTrue(document.IsNormalized());
    }

    [TestMethod("Verifica normalização de CNPJ numérico")]
    public void It_ShouldNormalizeNumericCnpj()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "73.678.476/0001-50");

        if (!document.IsNormalized())
            document.Normalize();

        Assert.AreEqual("73678476000150", document.Number);
        Assert.IsTrue(document.IsNormalized());
    }

    [TestMethod("Verifica normalização de CNPJ alfanumérico")]
    public void It_ShouldNormalizeAlphanumericCnpj()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "HT.W3P.JI6/3gn4-04");

        if (!document.IsNormalized())
            document.Normalize();

        Assert.AreEqual("HTW3PJI63GN404", document.Number);
        Assert.IsTrue(document.IsNormalized());
    }

    #endregion

    #region Validações

    [TestMethod("Verifica CPF inválido")]
    public void It_ShouldReturnFalse_WhenCpfIsInvalid()
    {
        var document = new PersonalDocument(DocumentType.Cpf, "123.456.789-00");

        Assert.IsFalse(Validator.Validate(document));
    }

    [TestMethod("Verifica CPF válido com pontuação")]
    public void It_ShouldReturnTrue_WhenCpfIsValidWithDots()
    {
        var document = new PersonalDocument(DocumentType.Cpf, "585.331.480-70");
        document.Normalize();

        Assert.IsTrue(Validator.Validate(document));
    }

    [TestMethod("Verifica CPF válido sem pontuação")]
    public void It_ShouldReturnTrue_WhenCpfIsValid()
    {
        var document = new PersonalDocument(DocumentType.Cpf, "11706848072");

        Assert.IsTrue(Validator.Validate(document));
    }

    [TestMethod("Verifica CNPJ inválido")]
    public void It_ShouldReturnFalse_WhenCnpjIsInvalid()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "12.345.678/0001-00");

        Assert.IsFalse(Validator.Validate(document));
    }

    [TestMethod("Verifica CNPJ válido com pontuação")]
    public void It_ShouldReturnTrue_WhenCnpjIsValidWithDots()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "66.413.818/0001-44");
        document.Normalize();

        Assert.IsTrue(Validator.Validate(document));
    }

    [TestMethod("Verifica CNPJ válido sem pontuação")]
    public void It_ShouldReturnTrue_WhenCnpjIsValid()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "11222333000181");

        Assert.IsTrue(Validator.Validate(document));
    }

    [TestMethod("Verifica CNPJ alfanumérico inválido")]
    public void It_ShouldReturnFalse_WhenCnpjContainsLetters()
    {
        var document = new PersonalDocument(DocumentType.Cnpj, "11.222.333/ABC1-81");

        Assert.IsFalse(Validator.Validate(document));
    }

    #endregion
}
