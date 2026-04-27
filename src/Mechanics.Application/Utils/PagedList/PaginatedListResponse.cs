namespace Mechanics.Application.Utils.PagedList;

public abstract class PaginatedListResponse<T>(IEnumerable<T> items, int totalCount)
{
    public IEnumerable<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
}
