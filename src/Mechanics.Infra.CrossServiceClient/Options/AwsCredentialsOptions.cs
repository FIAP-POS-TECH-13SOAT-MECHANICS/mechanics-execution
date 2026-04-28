namespace Mechanics.Infra.CrossServiceClient.Options;

public class AwsCredentialsOptions
{
    public required string Region { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretAccessKey { get; init; }
    public required string SessionToken { get; init; }
}
