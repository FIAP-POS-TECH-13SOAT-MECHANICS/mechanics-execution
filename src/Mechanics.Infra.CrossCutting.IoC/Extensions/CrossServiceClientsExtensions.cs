using Mechanics.Application.Identity.Services;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Infra.CrossServiceClient.Extensions;
using Mechanics.Infra.CrossServiceClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class CrossServiceClientsExtensions
{
    public static IServiceCollection AddCrossServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(nameof(CrossServiceClients)).Get<CrossServiceClients>()!;

        services.AddHttpClients(configuration)
            .AddCrossServiceClient<IUserService, UserService>(options.IdentityBaseUrl);

        return services;
    }
}
