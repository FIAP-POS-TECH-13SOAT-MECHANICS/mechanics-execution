using Mechanics.Application.Customers.Requests;
using Mechanics.Domain.Customers;

namespace Mechanics.Tests.Unit.Mocks;

public static class CustomerMocks
{
    public static CreateIndividualCustomerRequest BuildCreateRequestPf(string? cpf = null) => new()
    {
        FullName = "Joao da Silva",
        Email = "joao@teste.com",
        CpfNumber = cpf ?? "11144477735",
    };

    public static CreateBusinessCustomerRequest BuildCreateRequestPj(string? cnpj = null) => new()
    {
        CompanyName = "Empresa XYZ Ltda",
        CnpjNumber = cnpj ?? "11444777000161",
        ResponsibleFullName = "Responsavel XYZ",
        ResponsibleEmail = "contato@xyz.com",
        ResponsibleCpfNumber = "11144477735",
    };

    public static CreateIndividualCustomerRequest BuildInvalidCreateRequest() => new()
    {
        FullName = "",
        Email = "",
        CpfNumber = "00000000000",
    };

    public static UpdateCustomerRequest BuildUpdateRequest() => new()
    {
        Name = "Maria Joaquina",
        Email = "maria@teste.com",
        Document = new PersonalDocumentRequest
        {
            Type = DocumentType.Cpf,
            Number = "52998224725",
        },
    };

    public static UpdateCustomerRequest BuildInvalidUpdateRequest() => new()
    {
        Name = "",
        Email = "",
        Document = new PersonalDocumentRequest
        {
            Type = DocumentType.Cpf,
            Number = "00000000000",
        },
    };

    public static Customer CreateCustomerPf(Guid id) => new()
    {
        Id = id,
        Name = "Joao da Silva",
        Email = "joao@EXAMPLE.com",
        Document = new PersonalDocument(DocumentType.Cpf, "11144477735"),
    };

    public static Customer CreateCustomerPj(Guid id) => new()
    {
        Id = id,
        Name = "Empresa XYZ Ltda",
        Email = "contato@xyz.com",
        Document = new PersonalDocument(DocumentType.Cnpj, "11444777000161"),
    };
}
