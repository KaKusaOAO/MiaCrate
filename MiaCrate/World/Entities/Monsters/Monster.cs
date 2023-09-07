namespace MiaCrate.World.Entities.Monsters;

public abstract class Monster : PathfinderMob, IEnemy
{
    protected Monster(IEntityType type, Level level) : base(type, level)
    {
    }
}