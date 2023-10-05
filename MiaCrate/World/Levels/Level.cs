using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.Server;
using MiaCrate.World.Blocks;
using MiaCrate.World.Damages;
using MiaCrate.World.Dimensions;
using MiaCrate.World.Entities;
using MiaCrate.World.Phys;
using Mochi.Utils;

namespace MiaCrate.World;

public abstract class Level : ILevelAccessor, IDisposable
{
    private readonly IWritableLevelData _levelData;
    private readonly Func<IProfilerFiller> _profiler;
    private readonly IResourceKey<DimensionType> _dimensionTypeId;

    protected float oRainLevel;
    protected float rainLevel;
    protected float oThunderLevel;
    protected float thunderLevel;
    
    public IHolder<DimensionType> DimensionTypeRegistration { get; }
    public IResourceKey<Level> Dimension { get; }
    public DimensionType DimensionType => DimensionTypeRegistration.Value;

    public bool IsClientSide { get; }
    
    public BlockPos SharedSpawnPos { get; }
    
    public float SharedSpawnAngle { get; }

    public int Height => throw new NotImplementedException();

    public int MinBuildHeight => throw new NotImplementedException();
    public LevelLightEngine LightEngine => throw new NotImplementedException();

    public WorldBorder WorldBorder => throw new NotImplementedException();

    public ILevelData LevelData => throw new NotImplementedException();

    public GameServer? Server => throw new NotImplementedException();

    public long DayTime => throw new NotImplementedException();

    public abstract ChunkSource ChunkSource { get; }

    public IRandomSource Random { get; } = IRandomSource.Create();

    public abstract List<Player> Players { get; }
    public DamageSources DamageSources { get; }

    protected Level(IWritableLevelData levelData, IResourceKey<Level> dimension, IRegistryAccess registryAccess,
        IHolder<DimensionType> dimensionTypeRegistration, Func<IProfilerFiller> profiler, bool isClientSide,
        bool isDebug, long l, int i)
    {
        _profiler = profiler;
        _levelData = levelData;
        DimensionTypeRegistration = dimensionTypeRegistration;
        _dimensionTypeId = dimensionTypeRegistration.UnwrapKey().OrElseGet(() =>
            throw new ArgumentException($"Dimension must be registered, got {dimensionTypeRegistration}"));

        var dimensionType = dimensionTypeRegistration.Value;
        Dimension = dimension;
        IsClientSide = isClientSide;
        
        Util.LogFoobar();
    }
    
    public ChunkAccess GetChunk(int x, int z, ChunkStatus status, bool bl)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public BlockEntity? GetBlockEntity(BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public BlockState GetBlockState(BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public float GetShade(Direction direction, bool bl)
    {
        throw new NotImplementedException();
    }

    public int GetBlockTint(BlockPos pos, ColorResolver resolver)
    {
        throw new NotImplementedException();
    }

    public IBlockGetter GetChunkForCollisions(int x, int z)
    {
        throw new NotImplementedException();
    }

    public float GetRainLevel(float f) => Mth.Lerp(oRainLevel, rainLevel, f);

    public float GetThunderLevel(float f) => Mth.Lerp(oThunderLevel, thunderLevel, f) * GetRainLevel(f);

    public void SetRainLevel(float level)
    {
        var g = Math.Clamp(level, 0, 1);
        oRainLevel = rainLevel = g;
    }

    public void SetThunderLevel(float level)
    {
        
        var g = Math.Clamp(level, 0, 1);
        oThunderLevel = thunderLevel = g;
    }

    public List<Entity> GetEntities(Entity? entity, AABB aabb, Predicate<Entity> predicate)
    {
        throw new NotImplementedException();
    }

    public IHolder<Biome> GetNoiseBiome(int x, int y, int z)
    {
        throw new NotImplementedException();
    }

    public bool IsStateAtPosition(BlockPos pos, Predicate<BlockState> predicate)
    {
        throw new NotImplementedException();
    }

    public bool SetBlock(BlockPos pos, BlockState state, int i, int j)
    {
        throw new NotImplementedException();
    }

    public bool RemoveBlock(BlockPos pos, bool bl)
    {
        throw new NotImplementedException();
    }

    public bool DestroyBlock(BlockPos pos, bool bl, Entity? entity, int i)
    {
        throw new NotImplementedException();
    }

    public virtual bool AddFreshEntity(Entity entity) => ILevelWriter.LevelWriterDefaults.AddFreshEntity(this, entity);

    public virtual void OnBlockStateChange(BlockPos pos, BlockState oldState, BlockState newState) {}
}