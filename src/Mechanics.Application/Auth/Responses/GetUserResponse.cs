namespace Mechanics.Application.Auth.Responses;

public class GetUserResponse
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string CpfNumber { get; init; }
    public required RoleResponse Role { get; init; }
}
