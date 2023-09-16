namespace MiaCrate.World.Entities;

public class Horse : AbstractHorse
{
    public Horse(IEntityType type, Level level) : base(type, level)
    {
    }
}

public abstract class AbstractHorse : Animal
{
    protected AbstractHorse(IEntityType type, Level level) : base(type, level)
    {
    }
}