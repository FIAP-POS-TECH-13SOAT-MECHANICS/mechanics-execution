using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;

namespace Mechanics.Application.Auth;

public class AuthMapperProfile : Profile
{
    public AuthMapperProfile()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<CreateUserForCustomerRequest, User>();
        CreateMap<User, GetUserResponse>();

        CreateMap<Role, RoleResponse>();
    }
}
