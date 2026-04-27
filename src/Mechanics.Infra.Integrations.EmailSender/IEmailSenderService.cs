namespace Mechanics.Infra.Integrations.EmailSender;

public interface IEmailSenderService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
