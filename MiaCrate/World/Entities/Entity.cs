using System.Diagnostics.CodeAnalysis;
using MiaCrate.Core;
using MiaCrate.Net.Data;

namespace MiaCrate.World.Entities;

public abstract class Entity : IEntityAccess
{
    protected static IEntityDataAccessor<byte> DataSharedFlags { get; } =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);
    private static IEntityDataAccessor<int> DataAirSupply { get; } =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);
    
    public int Id => throw new NotImplementedException();

    public Uuid Uuid { get; set; }
    public BlockPos BlockPosition => throw new NotImplementedException();
    public Level Level { get; }
    protected SynchedEntityData EntityData { get; }

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Entity(IEntityType type, Level level)
    {
        Level = level;
        
        // Synched data
        EntityData = new SynchedEntityData();
        EntityData.Define(DataSharedFlags, (byte)0);
        EntityData.Define(DataAirSupply, MaxAirSupply);
        DefineSynchedData();
    }
    
    protected abstract void DefineSynchedData();

    public virtual int MaxAirSupply => 300;
}