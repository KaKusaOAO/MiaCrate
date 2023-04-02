namespace Mochi.World.Entities;

public abstract class LivingEntity : Entity
{
    protected LivingEntity(IEntityType type, ILevel level) : base(type, level)
    {
    }
}