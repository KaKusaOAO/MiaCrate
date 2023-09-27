using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World.Dimensions;
using Mochi.Utils;

namespace MiaCrate.World;

public abstract class Level : IDisposable
{
    private readonly IWritableLevelData _levelData;
    private readonly Func<IProfilerFiller> _profiler;
    private readonly IResourceKey<DimensionType> _dimensionTypeId;
    
    public IHolder<DimensionType> DimensionTypeRegistration { get; }
    public IResourceKey<Level> Dimension { get; }
    public DimensionType DimensionType => DimensionTypeRegistration.Value;
    
    public bool IsClientSide { get; }
    
    public BlockPos SharedSpawnPos { get; }
    
    public float SharedSpawnAngle { get; }

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
        
        
    }

    public void Dispose()
    {
    }
}