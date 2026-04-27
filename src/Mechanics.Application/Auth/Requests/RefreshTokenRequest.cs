namespace Mechanics.Application.Auth.Requests;

public class RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
