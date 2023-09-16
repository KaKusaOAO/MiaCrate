namespace MiaCrate.World.Entities;

public class Wolf : TameableAnimal, INeutralMob
{
    public Wolf(IEntityType type, Level level) : base(type, level)
    {
    }
}