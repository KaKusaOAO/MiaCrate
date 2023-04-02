namespace Mochi.World.Entities;

public abstract class Animal : AgeableMob
{
    protected Animal(IEntityType type, ILevel level) : base(type, level)
    {
    }
}