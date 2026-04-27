namespace Mechanics.Application.Customers.Requests;

public class CreateBusinessCustomerRequest
{
    /// <summary>
    ///     Nome fantasia da empresa.
    /// </summary>
    public required string CompanyName { get; init; }

    /// <summary>
    ///     CNPJ da empresa.
    /// </summary>
    public required string CnpjNumber { get; init; }

    /// <summary>
    ///     Nome completo do responsável.
    /// </summary>
    public required string ResponsibleFullName { get; init; }

    /// <summary>
    ///     E-mail de contato do responsável.
    /// </summary>
    public required string ResponsibleEmail { get; init; }

    /// <summary>
    ///     CPF do responsável.
    /// </summary>
    public required string ResponsibleCpfNumber { get; init; }
}
