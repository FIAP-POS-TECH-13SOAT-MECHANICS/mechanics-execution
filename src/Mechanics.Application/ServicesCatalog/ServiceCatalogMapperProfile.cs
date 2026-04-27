using AutoMapper;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.ServicesCatalog.Responses;
using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Application.ServicesCatalog;

public class ServiceCatalogMapperProfile : Profile
{
    public ServiceCatalogMapperProfile()
    {
        // Mapeamento da requisição de criação para a entidade
        CreateMap<CreateServiceCatalogRequest, ServiceCatalog>();

        // Mapeamento da entidade para a resposta
        CreateMap<ServiceCatalog, GetServiceCatalogResponse>();

        // Mapeamento da requisição de atualização para a entidade (parcial)
        CreateMap<UpdateServiceCatalogRequest, ServiceCatalog>();
    }
}
