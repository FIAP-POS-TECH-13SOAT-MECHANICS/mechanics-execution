using System.ComponentModel.DataAnnotations;

namespace Mechanics.Application.Auth.Requests;

public class CreateUserRequest
{
    /// <summary>
    ///     Nome completo do usuário.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    ///     CPF do usuário.
    /// </summary>
    public required string CpfNumber { get; init; }

    /// <summary>
    ///     E-mail do usuário.
    /// </summary>
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    ///     Perfil de acesso associado ao usuário.
    /// </summary>
    public required Guid RoleId { get; init; }
}
