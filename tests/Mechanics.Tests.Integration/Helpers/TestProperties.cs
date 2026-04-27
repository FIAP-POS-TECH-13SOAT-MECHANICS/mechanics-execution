using Amazon.Runtime;
using Amazon.SQS;
using DotNet.Testcontainers.Containers;
using Testcontainers.MsSql;

namespace Mechanics.Tests.Integration.Helpers;

[TestClass]
public static class TestProperties
{
    public static ApplicationFactory Factory { get; private set; } = null!;
    private static MsSqlContainer _msSqlContainer = null!;
    private static IContainer _smtpServerContainer = null!;
    private static IContainer _awsClientContainer = null!;

    [AssemblyInitialize]
    public static async Task Setup(TestContext context)
    {
        // desativa a telemetria
        Environment.SetEnvironmentVariable("Datadog__OtlpEndpoint", "http://localhost");

        await Task.WhenAll(
            SetupDatabase(context),
            SetupSmtpServer(context),
            SetupAwsClient(context)
        );

        Factory = new ApplicationFactory();
    }

    public static Uri GetEmailClientUri() =>
        new UriBuilder("http", _smtpServerContainer.Hostname, _smtpServerContainer.GetMappedPublicPort(8025)).Uri;

    public static AmazonSQSClient GetSqsClient()
    {
        var port = _awsClientContainer.GetMappedPublicPort(4566);
        return new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig { ServiceURL = $"http://localhost:{port}" });
    }

    private static async Task SetupDatabase(TestContext context)
    {
        _msSqlContainer = new TestDatabaseContainer().Container;
        await _msSqlContainer.StartAsync(context.CancellationTokenSource.Token);

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _msSqlContainer.GetConnectionString());
    }

    private static async Task SetupSmtpServer(TestContext context)
    {
        _smtpServerContainer = new TestSmtpServerContainer().Container;
        await _smtpServerContainer.StartAsync(context.CancellationTokenSource.Token);

        var smtpPort = _smtpServerContainer.GetMappedPublicPort(1025).ToString();
        Environment.SetEnvironmentVariable("EmailSenderOptions__SmtpServer", _smtpServerContainer.Hostname);
        Environment.SetEnvironmentVariable("EmailSenderOptions__SmtpPort", smtpPort);
        Environment.SetEnvironmentVariable("EmailSenderOptions__SslRequired", "false");
        Environment.SetEnvironmentVariable("EmailSenderOptions__UserName", TestSmtpServerContainer.UserName);
        Environment.SetEnvironmentVariable("EmailSenderOptions__Password", TestSmtpServerContainer.Password);
    }

    private static async Task SetupAwsClient(TestContext context)
    {
        _awsClientContainer = new TestAwsClientContainer().Container;
        await _awsClientContainer.StartAsync(context.CancellationTokenSource.Token);

        Environment.SetEnvironmentVariable("AwsCredentials__AccessKey", "test");
        Environment.SetEnvironmentVariable("AwsCredentials__SecretAccessKey", "test");
        Environment.SetEnvironmentVariable("AwsCredentials__SessionToken", "test");
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        await Factory.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
        await _smtpServerContainer.DisposeAsync();
        await _awsClientContainer.DisposeAsync();
    }
}
