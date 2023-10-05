using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World;
using MiaCrate.World.Dimensions;
using MiaCrate.World.Entities;

namespace MiaCrate.Server.Levels;

public class ServerLevel : Level, IWorldGenLevel
{
    public static BlockPos EndSpawnPoint { get; } = new(100, 50, 0);

    public ServerLevel(IWritableLevelData levelData, IResourceKey<Level> dimension, IRegistryAccess registryAccess,
        IHolder<DimensionType> dimensionTypeRegistration, Func<IProfilerFiller> profiler, bool isClientSide,
        bool isDebug, long l, int i) : base(levelData, dimension, registryAccess, dimensionTypeRegistration, profiler,
        isClientSide, isDebug, l, i)
    {
    }

    public long Seed => throw new NotImplementedException();

    public void SetCurrentlyGenerating(Func<string>? currentlyGenerating)
    {
        throw new NotImplementedException();
    }

    public override bool AddFreshEntity(Entity entity) => AddEntity(entity);

    private bool AddEntity(Entity entity)
    {
        throw new NotImplementedException();
    }
}