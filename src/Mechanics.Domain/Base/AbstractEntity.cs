namespace Mechanics.Domain.Base;

/// <summary>
///     Classe base para entidades de domínio.
/// </summary>
public abstract class AbstractEntity
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }

    public override bool Equals(object? obj)
    {
        var entity = obj as AbstractEntity;
        return entity != null && entity.Id == Id;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
}
