using Newtonsoft.Json;

namespace Mechanics.Tests.Unit.Helpers;

public static class CloneHelper
{
    public static T DeepClone<T>(T obj)
    {
        var serialized = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(serialized)!;
    }
}
