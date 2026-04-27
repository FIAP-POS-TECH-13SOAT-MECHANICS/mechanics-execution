using AutoMapper;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders;

public class WorkOrderMapperProfile : Profile
{
    public WorkOrderMapperProfile()
    {
        CreateMap<WorkOrderProduct, WorkOrderProductResponse>();
        CreateMap<WorkOrder, GetWorkOrderResponse>()
            .ForMember(dest => dest.ServiceCatalogIds,
                opt => opt.MapFrom(src =>
                    src.ServiceCatalog != null ? src.ServiceCatalog.Select(s => s.Id) : Enumerable.Empty<Guid>()));
    }
}
