using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Auth.Requests;

public class GetUsersRequest : PaginatedListRequest
{
    public string Name { get; init; } = "";
}
