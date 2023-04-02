using System.Diagnostics.CodeAnalysis;
using MiaCrate.Net.Data;
using MiaCrate.World.Entities.AI;

namespace MiaCrate.World.Entities;

public abstract class Mob : LivingEntity
{
    private static readonly IEntityDataAccessor<byte> _dataMobFlags =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);

    protected GoalSelector GoalSelector { get; } = new();
    protected GoalSelector TargetSelector { get; } = new();
    
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Mob(IEntityType type, ILevel? level) : base(type, level!)
    {
        if (level is { IsClientSide: false })
        {
            RegisterGoals();
        }
    }

    protected override void DefineSynchedData()
    {
        base.DefineSynchedData();
        EntityData.Define(_dataMobFlags, (byte) 0);
    }
    
    protected virtual void RegisterGoals() {}
}