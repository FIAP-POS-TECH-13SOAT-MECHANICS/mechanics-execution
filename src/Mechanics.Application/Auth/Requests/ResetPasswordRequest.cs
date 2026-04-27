namespace Mechanics.Application.Auth.Requests;

public class ResetPasswordRequest
{
    /// <summary>
    ///     O CPF do usuário cadastrado.
    /// </summary>
    public required string CpfNumber { get; init; }
}
