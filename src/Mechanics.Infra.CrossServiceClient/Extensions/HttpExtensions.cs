using Amazon;
using Amazon.Lambda;
using Amazon.Runtime;
using Mechanics.Infra.CrossServiceClient.Options;
using Mechanics.Infra.CrossServiceClient.ServiceTokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossServiceClient.Extensions;

public static class HttpExtensions
{
    public static ServiceClientBuilder AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("AwsCredentials").Get<AwsCredentialsOptions>()!;

        services.AddSingleton(new AmazonLambdaClient(
            new SessionAWSCredentials(options.AccessKey, options.SecretAccessKey, options.SessionToken),
            new AmazonLambdaConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region) }));
        services.AddSingleton<ServiceTokenHandler>();

        return new ServiceClientBuilder(services);
    }
}
