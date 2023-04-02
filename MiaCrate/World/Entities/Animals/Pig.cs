using MiaCrate.Net.Data;
using MiaCrate.World.Entities.AI.Goals;

namespace MiaCrate.World.Entities.Animals;

public class Pig : Animal
{
    // Why are these lines AI-generated?
    private static readonly IEntityDataAccessor<bool> _dataSaddled =
        SynchedEntityData.DefineId<Entity, bool>(EntityDataSerializers.Bool);

    private static readonly IEntityDataAccessor<int> _dataBoostTime =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);
    
    public Pig(IEntityType type, ILevel level) : base(type, level)
    {
    }

    protected override void RegisterGoals()
    {
        base.RegisterGoals();
        GoalSelector.AddGoal(0, new FloatGoal(this));
    }

    protected override void DefineSynchedData()
    {
        base.DefineSynchedData();
        EntityData.Define(_dataSaddled, false);
        EntityData.Define(_dataBoostTime, 0);
    }
}