namespace Mochi.World.Entities;

public abstract class Mob : LivingEntity
{
    protected Mob(IEntityType type, ILevel level) : base(type, level)
    {
    }
}