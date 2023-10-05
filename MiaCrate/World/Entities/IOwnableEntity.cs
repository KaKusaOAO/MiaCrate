namespace MiaCrate.World.Entities;

public interface IOwnableEntity
{
    public Uuid? OwnerUuid { get; }
    
    public IEntityGetter Level { get; }

    public LivingEntity? Owner => OwnerUuid.HasValue ? Level.GetPlayerByUuid(OwnerUuid.Value) : null;
}