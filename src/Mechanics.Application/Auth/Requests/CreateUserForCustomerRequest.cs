using Mechanics.Application.Auth.Consumers;
using Mechanics.Application.Customers.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using System.ComponentModel.DataAnnotations;

namespace Mechanics.Application.Auth.Requests;

public class CreateUserForCustomerRequest
{
    /// <summary>
    ///     Nome completo do usuário.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    ///     CPF do usuário.
    /// </summary>
    public string CpfNumber { get; }

    /// <summary>
    ///     E-mail do usuário.
    /// </summary>
    [EmailAddress]
    public string Email { get; }

    /// <summary>
    ///     ID do cliente.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    ///     Perfil de acesso associado ao usuário.
    /// </summary>
    /// <remarks>Deve ser um perfil válido para clientes.</remarks>
    public Guid RoleId { get; }

    public CreateUserForCustomerRequest(Guid customerId, CreateIndividualCustomerRequest request)
    {
        FullName = request.FullName;
        CpfNumber = request.CpfNumber;
        Email = request.Email;
        CustomerId = customerId;
        RoleId = RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerUser).Id;
    }

    public CreateUserForCustomerRequest(CustomerCreatedEvent message)
    {
        FullName = message.FullName;
        CpfNumber = message.CpfNumber;
        Email = message.Email;
        CustomerId = message.CustomerId;
        RoleId = message.RoleId;
    }
}
