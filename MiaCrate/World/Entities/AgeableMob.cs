using MiaCrate.Net.Data;

namespace MiaCrate.World.Entities;

public abstract class AgeableMob : PathfinderMob
{
    private static readonly IEntityDataAccessor<bool> _dataIsBaby =
        SynchedEntityData.DefineId<Entity, bool>(EntityDataSerializers.Bool);
    
    protected AgeableMob(IEntityType type, ILevel level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        base.DefineSynchedData();
        EntityData.Define(_dataIsBaby, false);
    }
}