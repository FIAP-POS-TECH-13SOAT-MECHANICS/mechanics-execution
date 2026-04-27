using AutoMapper;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Responses;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Application.Vehicles;

public class VehiclesMapperProfile : Profile
{
    public VehiclesMapperProfile()
    {
        CreateMap<CreateVehicleRequest, Vehicle>()
            .ForMember(vehicle => vehicle.LicensePlate, opt
                => opt.MapFrom(src => new LicensePlate(src.LicensePlate)));

        CreateMap<Vehicle, GetVehicleResponse>()
            .ForMember(response => response.LicensePlate, o
                => o.MapFrom(vehicle => vehicle.LicensePlate.ToString()));
    }
}
