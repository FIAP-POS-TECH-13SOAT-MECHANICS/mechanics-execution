using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Customers.Responses;

public class GetCustomersResponse(IEnumerable<GetCustomerResponse> items, int totalCount)
    : PaginatedListResponse<GetCustomerResponse>(items, totalCount);
