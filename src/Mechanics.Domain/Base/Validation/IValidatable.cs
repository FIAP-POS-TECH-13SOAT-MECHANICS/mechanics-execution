namespace Mechanics.Domain.Base.Validation;

public interface IValidatable
{
    /// <summary>
    ///     Adiciona validações ao objeto.
    /// </summary>
    /// <param name="builder">Builder para adicionar as regras de validação.</param>
    /// <seealso cref="ValidationBuilder"/>
    void Validate(ValidationBuilder builder);
}
