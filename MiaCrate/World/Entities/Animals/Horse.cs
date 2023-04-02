namespace MiaCrate.World.Entities.Animals;

public class Horse : AbstractHorse
{
    public Horse(IEntityType type, ILevel level) : base(type, level)
    {
    }
}

public abstract class AbstractHorse : Animal
{
    protected AbstractHorse(IEntityType type, ILevel level) : base(type, level)
    {
    }
}