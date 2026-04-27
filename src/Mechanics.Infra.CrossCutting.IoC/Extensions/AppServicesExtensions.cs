using Mechanics.Application.Options;
using Mechanics.Application.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(config =>
        {
            config.LicenseKey = configuration.GetValue<string>("LuckyPennyLicenseKey");
            config.AddMaps(typeof(IAppService).Assembly);
        });

        var appServices = typeof(IAppService).Assembly.GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IAppService)));
        foreach (var appService in appServices)
            services.AddScoped(appService);

        services.Configure<AppInfo>(configuration.GetSection(nameof(AppInfo)));

        return services;
    }
}
