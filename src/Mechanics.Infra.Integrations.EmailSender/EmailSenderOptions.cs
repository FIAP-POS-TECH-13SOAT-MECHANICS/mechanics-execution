namespace Mechanics.Infra.Integrations.EmailSender;

public class EmailSenderOptions
{
    public bool Enabled { get; init; }
    public required string SmtpServer { get; init; }
    public int SmtpPort { get; init; }
    public bool SslRequired { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }

    public required EmailAddress SenderInformation { get; init; }

    public class EmailAddress
    {
        public required string Name { get; init; }
        public required string Address { get; init; }
    }
}
