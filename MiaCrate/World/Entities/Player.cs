namespace MiaCrate.World.Entities;

public abstract class Player : LivingEntity
{
    protected Player(IEntityType type, ILevel level) : base(type, level)
    {
    }
}