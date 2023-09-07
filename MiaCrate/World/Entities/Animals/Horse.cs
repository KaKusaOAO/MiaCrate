namespace MiaCrate.World.Entities.Animals;

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