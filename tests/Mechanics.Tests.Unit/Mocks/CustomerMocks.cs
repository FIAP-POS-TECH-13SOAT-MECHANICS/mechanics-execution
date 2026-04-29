using Mechanics.Domain.Customers;

namespace Mechanics.Tests.Unit.Mocks;

public static class CustomerMocks
{
    public static Customer CreateCustomerPf(Guid id) => new()
    {
        Id = id,
        Name = "Joao da Silva",
        Email = "joao@EXAMPLE.com",
    };

    public static Customer CreateCustomerPj(Guid id) => new()
    {
        Id = id,
        Name = "Empresa XYZ Ltda",
        Email = "contato@xyz.com",
    };
}
