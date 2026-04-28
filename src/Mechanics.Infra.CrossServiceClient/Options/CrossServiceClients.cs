namespace Mechanics.Infra.CrossServiceClient.Options;

public class CrossServiceClients
{
    public required string AuthTokenFunctionName { get; init; }
    public required string IdentityBaseUrl { get; init; }
}
