using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.Vehicles.Responses;

public class GetVehiclesResponse(IEnumerable<GetVehicleResponse> items, int totalCount)
    : PaginatedListResponse<GetVehicleResponse>(items, totalCount);
