using MiaCrate.Net.Data;

namespace MiaCrate.World.Entities;

public abstract class Raider : PatrollingMonster
{
    protected static IEntityDataAccessor<bool> DataIsCelebrating { get; } =
        SynchedEntityData.DefineId<Raider, bool>(EntityDataSerializers.Bool);

    public Raid? CurrentRaid { get; set; }
    public int Wave { get; set; }
    public bool CanJoinRaid { get; set; }

    public bool IsCelebrating
    {
        get => EntityData.Get(DataIsCelebrating);
        set => EntityData.Set(DataIsCelebrating, value);
    }
    
    public bool HasActiveRaid => CurrentRaid?.IsActive ?? false;

    protected Raider(IEntityType type, Level level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        base.DefineSynchedData();
        EntityData.Define(DataIsCelebrating, false);
    }

    public abstract void ApplyRaidBuffs(int i, bool bl);
}