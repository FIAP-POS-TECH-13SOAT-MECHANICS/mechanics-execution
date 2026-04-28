using Amazon.Lambda;
using Amazon.Lambda.Model;
using Mechanics.Infra.CrossServiceClient.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Mechanics.Infra.CrossServiceClient.ServiceTokenProvider;

public class ServiceTokenHandler(IAmazonLambda lambda, IOptions<CrossServiceClients> options) : DelegatingHandler
{
    private Lazy<Task<string>>? _token;
    private DateTime _expiresAt;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_expiresAt < DateTime.UtcNow)
            _token = null;
        _token ??= new Lazy<Task<string>>(async () => await GetTokenAsync(CancellationToken.None));

        var token = await _token.Value;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var response = await lambda.InvokeAsync(new InvokeRequest
        {
            FunctionName = options.Value.AuthTokenFunctionName,
            InvocationType = InvocationType.RequestResponse,
            Payload = """{"path":"/auth/service-token","httpMethod":"POST"}""",
        }, cancellationToken);

        using var reader = new StreamReader(response.Payload);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<ServiceTokenResponse>(json, _serializerOptions)!;

        _expiresAt = result.ExpirationDate.DateTime;

        return result.AccessToken;
    }
}
