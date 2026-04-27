namespace Mechanics.Domain.Base.Exceptions;

/// <summary>
///     Representa um erro de regra de negócio na aplicação.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}
