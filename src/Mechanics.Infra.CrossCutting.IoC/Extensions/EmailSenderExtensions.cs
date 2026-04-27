using Mechanics.Application.Notification.Services;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class EmailSenderExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSenderOptions = configuration.GetSection(nameof(EmailSenderOptions)).Get<EmailSenderOptions>()!;
        services.AddScoped<IEmailSenderService>(_ => new EmailSenderService(emailSenderOptions));

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
