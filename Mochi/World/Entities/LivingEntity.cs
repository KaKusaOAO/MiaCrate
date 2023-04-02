using Mochi.Net.Data;

namespace Mochi.World.Entities;

public abstract class LivingEntity : Entity
{
    protected static readonly IEntityDataAccessor<byte> DataLivingEntityFlags =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);

    private static readonly IEntityDataAccessor<int> _dataEffectColor =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);
    
    protected LivingEntity(IEntityType type, ILevel level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        EntityData.Define(DataLivingEntityFlags, (byte) 0);
        EntityData.Define(_dataEffectColor, 0);
    }
}