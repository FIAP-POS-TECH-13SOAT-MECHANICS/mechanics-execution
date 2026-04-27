namespace Mechanics.Application.Customers.Requests;

public class CreateIndividualCustomerRequest
{
    /// <summary>
    ///     Nome do cliente.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    ///     E-mail de contato do cliente.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    ///     Número do CPF do cliente.
    /// </summary>
    public required string CpfNumber { get; init; }
}
