namespace MiaCrate.World.Entities;

public abstract class Animal : AgeableMob
{
    protected Animal(IEntityType type, Level level) : base(type, level)
    {
    }
}