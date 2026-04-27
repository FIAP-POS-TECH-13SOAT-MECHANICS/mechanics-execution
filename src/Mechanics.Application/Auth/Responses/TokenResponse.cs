namespace Mechanics.Application.Auth.Responses;

public record TokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTimeOffset ExpirationDate { get; init; }
}
