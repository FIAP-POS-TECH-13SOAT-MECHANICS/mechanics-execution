namespace Mechanics.Infra.CrossServiceClient.ServiceTokenProvider;

internal class ServiceTokenResponse
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required DateTimeOffset ExpirationDate { get; init; }
}
