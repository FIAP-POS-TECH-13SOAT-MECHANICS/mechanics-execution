using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.ServicesCatalog.Responses;

public class GetServicesCatalogResponse(IEnumerable<GetServiceCatalogResponse> items, int totalCount)
    : PaginatedListResponse<GetServiceCatalogResponse>(items, totalCount);
