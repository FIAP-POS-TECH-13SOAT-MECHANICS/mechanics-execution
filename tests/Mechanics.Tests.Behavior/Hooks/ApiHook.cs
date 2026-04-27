using Reqnroll;

namespace Mechanics.Tests.Behavior.Hooks;

[Binding]
public class ApiHook
{
    public static ApplicationFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static void Setup()
    {
        Factory = new ApplicationFactory();
    }

    [AfterTestRun]
    public static async Task Cleanup()
    {
        await Factory.DisposeAsync();
    }
}
