using AutoMapper;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Customers.Responses;
using Mechanics.Domain.Customers;

namespace Mechanics.Application.Customers;

public class CustomersMapperProfile : Profile
{
    public CustomersMapperProfile()
    {
        CreateMap<PersonalDocumentRequest, PersonalDocument>()
            .ConstructUsing(src => new PersonalDocument(src.Type!.Value, src.Number));

        CreateMap<PersonalDocument, PersonalDocumentResponse>();
        CreateMap<Customer, GetCustomerResponse>();
    }
}
