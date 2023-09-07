namespace MiaCrate.World.Entities;

public abstract class Player : LivingEntity
{
    protected Player(IEntityType type, Level level) : base(type, level)
    {
    }
}