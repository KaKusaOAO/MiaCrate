using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World;
using MiaCrate.World.Dimensions;

namespace MiaCrate.Client.Multiplayer;

public class ClientLevel : Level
{
    public ClientLevel(IWritableLevelData levelData, IResourceKey<Level> dimension, IRegistryAccess registryAccess,
        IHolder<DimensionType> dimensionTypeRegistration, Func<IProfilerFiller> profiler, bool isClientSide,
        bool isDebug, long l, int i) : base(levelData, dimension, registryAccess, dimensionTypeRegistration, profiler,
        isClientSide, isDebug, l, i)
    {
    }
}