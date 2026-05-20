using Mechanics.Application.WorkOrdersApi.Responses;

namespace Mechanics.Tests.Unit.Mocks;

public static class CustomerMocks
{
    public static CustomerResponse CreateCustomerPf(Guid id) => new()
    {
        Id = id,
        Name = "Joao da Silva",
        Email = "joao@EXAMPLE.com",
        Document = new PersonalDocumentResponse
        {
            Type = "cpf",
            Number = "63077737078",
        },
    };

    public static CustomerResponse CreateCustomerPj(Guid id) => new()
    {
        Id = id,
        Name = "Empresa XYZ Ltda",
        Email = "contato@xyz.com",
        Document = new PersonalDocumentResponse
        {
            Type = "cnpj",
            Number = "23177806000155",
        },
    };
}
