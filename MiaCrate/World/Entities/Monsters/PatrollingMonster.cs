using MiaCrate.Core;

namespace MiaCrate.World.Entities;

public abstract class PatrollingMonster : Monster
{
    private bool _isPatrolLeader;
    
    public BlockPos? PatrolTarget { get; set; }
    
    protected bool IsPatrolling { get; set; }

    public bool IsPatrolLeader
    {
        get => _isPatrolLeader;
        set
        {
            _isPatrolLeader = value;
            IsPatrolling = true;
        }
    }
    
    public virtual bool CanBeLeader => true;
    public virtual bool CanJoinPatrol => true;

    protected PatrollingMonster(IEntityType type, Level level) : base(type, level)
    {
    }

    public void FindPatrolTarget()
    {
        PatrolTarget = BlockPosition.Offset(-500 + Random.Next(1000), 0, -500 + Random.Next(1000));
        IsPatrolling = true;
    }

    public override bool ShouldRemoveWhenFarAway(double d) => !IsPatrolling || d > 16384;
}