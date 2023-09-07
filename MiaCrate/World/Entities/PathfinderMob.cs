namespace MiaCrate.World.Entities;

public abstract class PathfinderMob : Mob
{
    protected PathfinderMob(IEntityType type, Level level) : base(type, level)
    {
    }
}