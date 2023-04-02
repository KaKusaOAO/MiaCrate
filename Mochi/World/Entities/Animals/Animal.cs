namespace Mochi.World.Entities.Animals;

public abstract class Animal : AgeableMob
{
    protected Animal(IEntityType type, ILevel level) : base(type, level)
    {
    }
}