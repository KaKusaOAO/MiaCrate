namespace Mochi.World.Entities;

public abstract class AgeableMob : PathfinderMob
{
    protected AgeableMob(IEntityType type, ILevel level) : base(type, level)
    {
    }
}