namespace Mechanics.Application.Options;

public class AppInfo
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string BaseUrl { get; init; }
    public required string RoutePrefix { get; init; }
    public required string Description { get; init; }
}
