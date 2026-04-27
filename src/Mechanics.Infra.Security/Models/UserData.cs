namespace Mechanics.Infra.Security.Models;

public class UserData
{
    public required Guid UserId { get; init; }
    public required string Role { get; init; }
    public Guid CustomerId { get; init; }

    public override string ToString()
    {
        return UserId.ToString();
    }
}
