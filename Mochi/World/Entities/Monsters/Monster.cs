namespace Mochi.World.Entities.Monsters;

public abstract class Monster : PathfinderMob, IEnemy
{
    protected Monster(IEntityType type, ILevel level) : base(type, level)
    {
    }
}