namespace MiaCrate.World.Entities.Projectiles;

public abstract class Projectile : Entity
{
    protected Projectile(IEntityType type, Level level) : base(type, level)
    {
    }
}