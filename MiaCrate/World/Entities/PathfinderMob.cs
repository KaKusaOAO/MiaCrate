namespace MiaCrate.World.Entities;

public abstract class PathfinderMob : Mob
{
    protected PathfinderMob(IEntityType type, ILevel level) : base(type, level)
    {
    }
}