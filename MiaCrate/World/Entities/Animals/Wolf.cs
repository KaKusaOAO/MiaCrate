using MiaCrate.Net.Data;
using MiaCrate.World.Entities.AI.Goals;

namespace MiaCrate.World.Entities;

public class Wolf : TameableAnimal, INeutralMob
{
    private static IEntityDataAccessor<bool> DataInterestedId { get; } =
        SynchedEntityData.DefineId<Wolf, bool>(EntityDataSerializers.Bool);
    private static IEntityDataAccessor<int> DataCollarColor { get; } =
        SynchedEntityData.DefineId<Wolf, int>(EntityDataSerializers.Int);
    private static IEntityDataAccessor<int> DataRemainingAngerTime { get; } =
        SynchedEntityData.DefineId<Wolf, int>(EntityDataSerializers.Int);

    private const float StartHealth = 8;
    private const float TameHealth = 20;

    private static UniformInt PersistentAngerTime { get; } = TimeUtil.RangeOfSeconds(20, 39);
    
    public int RemainingPersistentAngerTime
    {
        get => EntityData.Get(DataRemainingAngerTime);
        set => EntityData.Set(DataRemainingAngerTime, value);
    }

    public Uuid? PersistentAngerTarget { get; set; }
    
    public Wolf(IEntityType type, Level level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        base.DefineSynchedData();
        EntityData.Define(DataInterestedId, false);
        Util.LogFoobar();
        EntityData.Define(DataCollarColor, 0);
        EntityData.Define(DataRemainingAngerTime, 0);
    }

    protected override void RegisterGoals()
    {
        GoalSelector.AddGoal(1, new FloatGoal(this));
        Util.LogFoobar();
    }

    public void StartPersistentAngerTimer()
    {
        RemainingPersistentAngerTime = PersistentAngerTime.Sample(Random);
    }
}