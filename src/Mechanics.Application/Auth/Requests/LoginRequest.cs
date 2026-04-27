namespace Mechanics.Application.Auth.Requests;

public class LoginRequest
{
    /// <summary>
    ///     CPF do usuário.
    /// </summary>
    /// <example>12345678909</example>
    public required string CpfNumber { get; init; }

    /// <summary>
    ///     Senha da conta.
    /// </summary>
    /// <example>5eCre+Key</example>
    public required string Password { get; init; }
}
