using Mechanics.Domain.Customers;

namespace Mechanics.Application.Customers.Responses;

public class GetCustomerResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required PersonalDocumentResponse Document { get; init; }
}

public class PersonalDocumentResponse
{
    public required DocumentType Type { get; init; }
    public required string Number { get; init; }
}
