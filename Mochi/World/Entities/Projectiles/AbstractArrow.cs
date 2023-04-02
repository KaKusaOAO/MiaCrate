namespace Mochi.World.Entities.Projectiles;

public abstract class AbstractArrow : Projectile
{
    protected AbstractArrow(IEntityType type, ILevel level) : base(type, level)
    {
    }
}