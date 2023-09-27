﻿using System.Diagnostics.CodeAnalysis;
using MiaCrate.Commands;
using MiaCrate.Core;
using MiaCrate.Net.Data;
using MiaCrate.World.Phys;
using Mochi.Texts;

namespace MiaCrate.World.Entities;

public abstract class Entity : IEntityAccess, ICommandSource
{
    protected static IEntityDataAccessor<byte> DataSharedFlags { get; } =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);
    private static IEntityDataAccessor<int> DataAirSupply { get; } =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);

    public int Id => throw new NotImplementedException();

    public Uuid Uuid { get; set; }
    public Vec3 Position => throw new NotImplementedException();
    public BlockPos BlockPosition => throw new NotImplementedException();
    public Level Level { get; }
    protected SynchedEntityData EntityData { get; }

    public float EyeHeight { get; }

    public virtual int MaxAirSupply => 300;

    public bool AcceptsSuccess => throw new NotImplementedException();

    public bool AcceptsFailure => true;

    public bool ShouldInformAdmins => true;
    
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Entity(IEntityType type, Level level)
    {
        Level = level;
        
        // Synched data
        EntityData = new SynchedEntityData();
        EntityData.Define(DataSharedFlags, (byte)0);
        EntityData.Define(DataAirSupply, MaxAirSupply);
        DefineSynchedData();

        EyeHeight = 0;
        Util.LogFoobar();
    }
    
    protected abstract void DefineSynchedData();
    
    public virtual void SendSystemMessage(IComponent component)
    {
        
    }
}