namespace MiaCrate.World.Entities.Animals;

public abstract class TameableAnimal : Animal, IOwnableEntity
{
    protected TameableAnimal(IEntityType type, Level level) : base(type, level)
    {
    }

    public Guid OwnerId { get; }
    public Entity Owner { get; }
}