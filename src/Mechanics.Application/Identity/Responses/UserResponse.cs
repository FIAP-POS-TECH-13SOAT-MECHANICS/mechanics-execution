namespace Mechanics.Application.Identity.Responses;

public class UserResponse
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string CpfNumber { get; init; }
    public required string Email { get; init; }
    public required RoleResponse Role { get; init; }
}

public class RoleResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
