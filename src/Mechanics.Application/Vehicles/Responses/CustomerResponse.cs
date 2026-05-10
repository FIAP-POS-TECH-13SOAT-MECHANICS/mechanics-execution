namespace Mechanics.Application.Vehicles.Responses;

public class CustomerResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required PersonalDocumentResponse Document { get; init; }
}

public class PersonalDocumentResponse
{
    public required string Type { get; init; }
    public required string Number { get; init; }
}
