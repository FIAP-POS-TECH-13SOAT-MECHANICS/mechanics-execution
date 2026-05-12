using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Model;
using Mechanics.Infra.CrossServiceClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Mechanics.Infra.CrossServiceClient.ServiceTokenProvider;

public class AuthTokenService(IAmazonLambda lambda, IOptions<CrossServiceClients> options, IConfiguration configuration)
{
    private Lazy<Task<string>>? _token;
    private DateTime _expiresAt;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_expiresAt < DateTime.UtcNow)
            _token = null;
        _token ??= new Lazy<Task<string>>(async () => await FetchToken(CancellationToken.None));

        return await _token.Value;
    }

    private async Task<string> FetchToken(CancellationToken cancellationToken)
    {
        var invokeResponse = await lambda.InvokeAsync(new InvokeRequest
        {
            FunctionName = options.Value.AuthTokenFunctionName,
            InvocationType = InvocationType.RequestResponse,
            Payload = CreateRequest(configuration["AppInfo:Name"]!),
        }, cancellationToken);

        if (invokeResponse.HttpStatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException(
                $"Failed to retrieve token from auth function. Status code: {invokeResponse.HttpStatusCode}");

        using var reader = new StreamReader(invokeResponse.Payload);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<APIGatewayHttpApiV2ProxyResponse>(json, _serializerOptions)!;

        var content = JsonSerializer.Deserialize<ServiceTokenResponse>(response.Body, _serializerOptions);
        if (content is null)
            throw new ApplicationException($"Failed to deserialize token response from auth function. Response: {json}");

        _expiresAt = content.ExpirationDate.DateTime;

        return content.AccessToken;
    }

    private string CreateRequest(string serviceName)
    {
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            Version = "2.0",
            RouteKey = "POST /auth/service-token",
            RawPath = "/auth/service-token",
            RawQueryString = "",
            Headers = new Dictionary<string, string>
            {
                ["content-type"] = "application/json",
            },
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = "POST",
                    Path = "/auth/service-token",
                    Protocol = "HTTP/1.1",
                },
                RouteKey = "POST /auth/service-token",
                Stage = "$default",
                RequestId = Guid.NewGuid().ToString(),
            },
            Body = $$"""{"serviceName": "{{serviceName}}"}""",
            IsBase64Encoded = false,
        };

        return JsonSerializer.Serialize(request, _serializerOptions);
    }
}
