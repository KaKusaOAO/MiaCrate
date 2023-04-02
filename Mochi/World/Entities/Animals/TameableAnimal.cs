namespace Mochi.World.Entities.Animals;

public abstract class TameableAnimal : Animal, IOwnableEntity
{
    protected TameableAnimal(IEntityType type, ILevel level) : base(type, level)
    {
    }

    public Guid OwnerId { get; }
    public Entity Owner { get; }
}