namespace MiaCrate.World.Entities.Projectiles;

public abstract class Projectile : Entity
{
    protected Projectile(IEntityType type, ILevel level) : base(type, level)
    {
    }
}