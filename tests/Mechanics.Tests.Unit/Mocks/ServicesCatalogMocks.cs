using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Tests.Unit.Mocks;

public static class ServicesCatalogMocks
{
    public static CreateServiceCatalogRequest BuildCreateRequest() => new()
    {
        Name = "Troca de Óleo",
        Description = "Troca de óleo sintético com filtro",
        BasePrice = 199.90m,
        AverageTime = 45,
        Status = ServiceCatalogStatusType.Active,
    };

    public static CreateServiceCatalogRequest BuildInvalidCreateRequest() => new()
    {
        Name = "",
        Description = "",
        BasePrice = -50,
        AverageTime = 0,
        Status = ServiceCatalogStatusType.Active,
    };

    public static UpdateServiceCatalogRequest BuildUpdateRequest() => new()
    {
        Name = "Alinhamento",
        Description = "Alinhamento e balanceamento completo",
        BasePrice = 149.90m,
        AverageTime = 30,
        Status = ServiceCatalogStatusType.Active,
    };

    public static UpdateServiceCatalogRequest BuildInvalidUpdateRequest() => new()
    {
        Name = "",
        Description = "",
        BasePrice = -10,
        AverageTime = 0,
        Status = ServiceCatalogStatusType.Active,
    };

    public static ServiceCatalog CreateService(Guid id, string? name = null) => new()
    {
        Id = id,
        Name = name ?? "Freios",
        Description = "Revisão completa dos freios",
        BasePrice = 299.90m,
        AverageTime = 60,
        Status = ServiceCatalogStatusType.Active,
    };

    public static ServiceCatalog CreateInvalidService(Guid id) => new()
    {
        Id = id,
        Name = "",
        Description = "",
        BasePrice = -100,
        AverageTime = 0,
        Status = ServiceCatalogStatusType.Active,
    };

    public static ServiceCatalog CreateInactiveService(Guid id, string name = "Serviço Inativo") => new()
    {
        Id = id,
        Name = name,
        Description = "Serviço temporariamente indisponível",
        BasePrice = 120.00m,
        AverageTime = 40,
        Status = ServiceCatalogStatusType.Inactive,
    };

    public static ServiceCatalog CreateSearchableService(Guid id, string name, string description) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        BasePrice = 180.00m,
        AverageTime = 50,
        Status = ServiceCatalogStatusType.Active,
    };

    public static IEnumerable<ServiceCatalog> BuildSearchableServices()
    {
        yield return CreateSearchableService(Guid.NewGuid(), "Troca de Óleo", "Óleo sintético para SUV");
        yield return CreateSearchableService(Guid.NewGuid(), "Geometria", "Geometria e pneus");
        yield return CreateSearchableService(Guid.NewGuid(), "Suspensão", "Suspensão reforçada para veículos pesados");
    }

    public static IEnumerable<ServiceCatalog> BuildManyServices(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            yield return new ServiceCatalog
            {
                Id = Guid.NewGuid(),
                Name = $"Serviço {i}",
                Description = $"Descrição do serviço {i}",
                BasePrice = 100 + i,
                AverageTime = 30 + i,
                Status = ServiceCatalogStatusType.Active,
            };
        }
    }
}
