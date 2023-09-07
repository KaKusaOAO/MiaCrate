namespace MiaCrate.World.Entities.Projectiles;

public abstract class AbstractArrow : Projectile
{
    protected AbstractArrow(IEntityType type, Level level) : base(type, level)
    {
    }
}