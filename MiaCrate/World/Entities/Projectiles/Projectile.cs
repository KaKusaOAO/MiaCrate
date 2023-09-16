namespace MiaCrate.World.Entities;

public abstract class Projectile : Entity
{
    protected Projectile(IEntityType type, Level level) : base(type, level)
    {
    }
}