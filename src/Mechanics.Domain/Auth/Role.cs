using Mechanics.Domain.Base;

namespace Mechanics.Domain.Auth;

public class Role : AbstractEntity
{
    public required string Name { get; init; }
}
