namespace MiaCrate.World.Entities;

public abstract class TameableAnimal : Animal, IOwnableEntity
{
    protected TameableAnimal(IEntityType type, Level level) : base(type, level)
    {
    }

    public Uuid? OwnerUuid => throw new NotImplementedException();

    IEntityGetter IOwnableEntity.Level => Level;
}