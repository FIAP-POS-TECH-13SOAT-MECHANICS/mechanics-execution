using Mechanics.Application.Options;
using Mechanics.Application.Utils;
using Mechanics.Application.Notification.Services;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            .Where(type => type.GetInterfaces().Contains(typeof(IAppService)) && type.IsClass && !type.IsAbstract);
        foreach (var appService in appServices)
        {
            services.AddScoped(appService);

            var serviceInterfaces = appService.GetInterfaces().Where(i => i != typeof(IAppService));
            foreach (var iface in serviceInterfaces)
            {
                if (!services.Any(sd => sd.ServiceType == iface))
                    services.AddScoped(iface, appService);
            }
        }

        // Configuração e registro do envio de e-mail
        services.Configure<EmailSenderOptions>(configuration.GetSection(nameof(EmailSenderOptions)));
        // Expõe EmailSenderOptions como instância para que EmailSenderService (que recebe EmailSenderOptions) possa ser construído
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSenderOptions>>().Value);

        // Registrar IEmailSenderService e IEmailService
        if (!services.Any(sd => sd.ServiceType == typeof(IEmailSenderService)))
            services.AddSingleton<IEmailSenderService, EmailSenderService>();

        if (!services.Any(sd => sd.ServiceType == typeof(IEmailService)))
            services.AddScoped<IEmailService, EmailService>();

        services.Configure<AppInfo>(configuration.GetSection(nameof(AppInfo)));

        return services;
    }
}
