using Mechanics.Application.Customers.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;

namespace Mechanics.Application.Customers.Events;

public class CustomerCreatedEvent
{
    public string FullName { get; }
    public string CpfNumber { get; }
    public string Email { get; }
    public Guid CustomerId { get; }
    public Guid RoleId { get; }
    public string RoleName { get; }

    public CustomerCreatedEvent(Guid customerId, CreateIndividualCustomerRequest request)
    {
        FullName = request.FullName;
        CpfNumber = request.CpfNumber;
        Email = request.Email;
        CustomerId = customerId;
        RoleId = RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerUser).Id;
        RoleName = RoleNames.CustomerUser;
    }

    public CustomerCreatedEvent(Guid customerId, CreateBusinessCustomerRequest request, bool isAdmin)
    {
        FullName = request.ResponsibleFullName;
        CpfNumber = request.ResponsibleCpfNumber;
        Email = request.ResponsibleEmail;
        CustomerId = customerId;
        RoleId = isAdmin
            ? RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerAdmin).Id
            : RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerUser).Id;
        RoleName = isAdmin ? RoleNames.CustomerAdmin : RoleNames.CustomerUser;
    }
}
