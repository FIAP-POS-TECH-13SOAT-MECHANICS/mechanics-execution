namespace Mechanics.Infra.CrossServiceClient.Options;

public class CrossServiceClients
{
    public required string AuthTokenFunctionName { get; init; }
    public required string IdentityBaseUrl { get; init; }
    public required string BillingBaseUrl { get; init; }
    public required string ExecutionBaseUrl { get; init; }
    public required string WorkOrdersBaseUrl { get; init; }
}
