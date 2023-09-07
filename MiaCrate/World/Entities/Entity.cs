using System.Diagnostics.CodeAnalysis;
using MiaCrate.Net.Data;

namespace MiaCrate.World.Entities;

public abstract class Entity
{
    protected static readonly IEntityDataAccessor<byte> DataSharedFlags =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);

    private static readonly IEntityDataAccessor<int> _dataAirSupply =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);

    protected SynchedEntityData EntityData { get; }

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Entity(IEntityType type, Level level)
    {
        Level = level;
        
        // Synched data
        EntityData = new SynchedEntityData();
        EntityData.Define(DataSharedFlags, (byte)0);
        EntityData.Define(_dataAirSupply, MaxAirSupply);
        DefineSynchedData();
    }
    
    public Guid Uuid { get; set; }
    public Level Level { get; }
    
    protected abstract void DefineSynchedData();

    public virtual int MaxAirSupply => 300;
}