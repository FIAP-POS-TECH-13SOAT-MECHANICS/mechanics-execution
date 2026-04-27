namespace Mechanics.Application.Utils.CommonResponses;

public class ListResponse<T>(IEnumerable<T> items)
{
    public IEnumerable<T> Items { get; } = items;
}
