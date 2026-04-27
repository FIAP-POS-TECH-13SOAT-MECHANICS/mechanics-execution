namespace Mechanics.Infra.Observability;

public class DatadogOptions
{
    public string? OtlpEndpoint { get; init; }
    public required string ApiKey { get; init; }
    public bool UseJsonLogs { get; init; }
}
