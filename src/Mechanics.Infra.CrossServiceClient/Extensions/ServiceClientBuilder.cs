using Mechanics.Infra.CrossServiceClient.ServiceTokenProvider;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossServiceClient.Extensions;

public class ServiceClientBuilder(IServiceCollection services)
{
    public ServiceClientBuilder AddCrossServiceClient<TInterface, T>(string serviceBaseUrl)
        where T : class, TInterface where TInterface : class
    {
        services.AddHttpClient<TInterface, T>(client => client.BaseAddress = new Uri(serviceBaseUrl))
            .AddHttpMessageHandler<ServiceTokenHandler>();

        return this;
    }
}
