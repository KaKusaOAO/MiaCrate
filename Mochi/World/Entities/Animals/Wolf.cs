namespace Mochi.World.Entities.Animals;

public class Wolf : TameableAnimal, INeutralMob
{
    public Wolf(IEntityType type, ILevel level) : base(type, level)
    {
    }
}