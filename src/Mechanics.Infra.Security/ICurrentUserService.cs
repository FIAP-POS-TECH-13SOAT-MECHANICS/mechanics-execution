using Mechanics.Infra.Security.Models;

namespace Mechanics.Infra.Security;

public interface ICurrentUserService
{
    UserData GetData();
}
