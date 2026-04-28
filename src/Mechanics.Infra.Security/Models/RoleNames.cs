namespace Mechanics.Infra.Security.Models;

public static class RoleNames
{
    /// <summary>
    ///     Acesso total ao sistema.
    /// </summary>
    public const string Administrator = "ADMINISTRATOR";

    /// <summary>
    ///     Permite cadastrar clientes e veículos e gerar ordens de serviços.
    /// </summary>
    public const string Attendant = "ATTENDANT";

    /// <summary>
    ///     Permite gerenciar ordens de serviços e realizar manutenções.
    /// </summary>
    public const string Mechanic = "MECHANIC";

    /// <summary>
    ///     Cliente comum.
    ///     Permite consultar ordens de serviço do seu cliente.
    /// </summary>
    public const string CustomerUser = "CUSTOMER_USER";

    /// <summary>
    ///     Cliente administrador.
    ///     Permite gerenciar outros usuários do mesmo cliente.
    /// </summary>
    public const string CustomerAdmin = "CUSTOMER_ADMIN";
}
