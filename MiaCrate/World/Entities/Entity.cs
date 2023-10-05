using System.Diagnostics.CodeAnalysis;
using MiaCrate.Commands;
using MiaCrate.Core;
using MiaCrate.Net.Data;
using MiaCrate.Tags;
using MiaCrate.World.Blocks;
using MiaCrate.World.Damages;
using MiaCrate.World.Phys;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.World.Entities;

public abstract class Entity : IEntityAccess, ICommandSource
{
    protected static IEntityDataAccessor<byte> DataSharedFlags { get; } =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);
    private static IEntityDataAccessor<int> DataAirSupply { get; } =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);

    private static int _entityCounter;
    private BlockState? _feetBlockState;
    private IEntityInLevelCallback _levelCallback = IEntityInLevelCallback.Null;
    private float _yRot, _xRot;
    
    public int Id { get; }
    public Uuid Uuid { get; set; }
    
    public ChunkPos ChunkPosition { get; private set; } = ChunkPos.Zero;
    public BlockPos BlockPosition { get; private set; } = BlockPos.Zero;
    public Vec3 Position { get; private set; } = Vec3.Zero;
    public double X => Position.X;
    public double Y => Position.Y;
    public double Z => Position.Z;

    public float YRot
    {
        get => _yRot;
        set
        {
            if (!float.IsFinite(value))
            {
                Logger.Warn($"Invalid entity rotation: {value}, discarding.");
                return;
            }

            _yRot = value;
        }
    }
    
    public float XRot
    {
        get => _xRot;
        set
        {
            if (!float.IsFinite(value))
            {
                Logger.Warn($"Invalid entity rotation: {value}, discarding.");
                return;
            }

            _xRot = value;
        }
    }
    
    public float YRotO { get; set; }
    public float XRotO { get; set; }
    
    public Level Level { get; protected set; }
    protected SynchedEntityData EntityData { get; }
    public float EyeHeight { get; }
    public virtual int MaxAirSupply => 300;
    public bool AcceptsSuccess => throw new NotImplementedException();
    public bool AcceptsFailure => true;
    public bool ShouldInformAdmins => true;
    protected IRandomSource Random { get; }
    public IEntityType Type { get; }
    public EntityRemovalReason? RemovalReason { get; private set; }
    public bool IsRemoved => RemovalReason.HasValue;
    public bool IsInvulnerable { get; private set; }
    public bool IsFireImmune => Type.IsFireImmune;
    public bool IsHurtMarked { get; set; }
    public DamageSources DamageSources => Level.DamageSources;
    public BlockState FeetBlockState => _feetBlockState ??= Level.GetBlockState(BlockPosition);
    
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Entity(IEntityType type, Level level)
    {
        Id = Interlocked.Increment(ref _entityCounter);
        Random = IRandomSource.Create();
        Type = type;
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

    public virtual void Kill()
    {
        Remove(EntityRemovalReason.Killed);
        throw new NotImplementedException();
    }

    public virtual void Tick() => BaseTick();

    public virtual void BaseTick()
    {
        _feetBlockState = null;

        XRotO = XRot;
        YRotO = YRot;

        throw new NotImplementedException();
    }

    public void Discard() => Remove(EntityRemovalReason.Discarded);

    public virtual void Remove(EntityRemovalReason reason) => SetRemoved(reason);

    public void SetRemoved(EntityRemovalReason reason)
    {
        throw new NotImplementedException();
    }

    public virtual bool Hurt(DamageSource source, float damage)
    {
        if (IsInvulnerableTo(source)) return false;

        MarkHurt();
        return false;
    }

    protected void MarkHurt()
    {
        IsHurtMarked = true;
    }

    public virtual bool IsInvulnerableTo(DamageSource source)
    {
        return IsRemoved ||
               IsInvulnerable && !source.Is(DamageTypeTags.BypassesInvulnerability) && !source.IsCreativePlayer ||
               source.Is(DamageTypeTags.IsFire) && IsFireImmune ||
               source.Is(DamageTypeTags.IsFall) && Type.Is(EntityTypeTags.FallDamageImmune);
    }

    public virtual void SendSystemMessage(IComponent component)
    {
        
    }
    
    public void SetLevelCallback(IEntityInLevelCallback callback)
    {
        _levelCallback = callback;
    }

    public virtual void SetPos(double x, double y, double z)
    {
        SetPosRaw(x, y, z);
        throw new NotImplementedException();
    }

    protected void SetRot(float yRot, float xRot)
    {
        YRot = yRot % 360f;
        XRot = xRot % 360f;
    }

    public void SetPosRaw(double x, double y, double z)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (Position.X == x && Position.Y == y && Position.Z == z) return;
        Position = new Vec3(x, y, z);
        
        var i = (int) Math.Floor(x);
        var j = (int) Math.Floor(y);
        var k = (int) Math.Floor(z);

        if (i != BlockPosition.X || j != BlockPosition.Y || k != BlockPosition.Z)
        {
            BlockPosition = new BlockPos(i, j, k);
            _feetBlockState = null;

            if (SectionPos.BlockToSectionCoord(i) != ChunkPosition.X ||
                SectionPos.BlockToSectionCoord(k) != ChunkPosition.Z)
            {
                ChunkPosition = new ChunkPos(BlockPosition);
            }
        }
        
        _levelCallback.OnMove();
    }
    
    public virtual void CheckDespawn() {}
    
    public virtual void OnClientRemoval() {}

    public virtual bool MayInteract(Level level, BlockPos pos) => true;

    protected virtual void PlayStepSound(BlockPos pos, BlockState state)
    {
        throw new NotImplementedException();
    }
    
    protected void LerpPositionAndRotationStep(int i, double d, double e, double f, double g, double h)
    {
        var j = 1.0 / i;
        
        var k = Mth.Lerp(X, d, j);
        var l = Mth.Lerp(Y, e, j);
        var m = Mth.Lerp(Z, f, j);

        var n = (float) Util.RotLerp(j, YRot, g);
        var o = Mth.Lerp(XRot, (float) h, (float) j);
        
        SetPos(k, l, m);
        SetRot(n, o);
    }
}