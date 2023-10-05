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

    public float GetSkyDarken(float f)
    {
        var g = this.GetTimeOfDay(f);
        var h = 1 - MathF.Cos(g * float.Pi * 2) * 2 + 0.2f;

        h = Math.Clamp(h, 0, 1);
        h = 1 - h;
        h *= 1 - GetRainLevel(f) * 5 / 16;
        h *= 1 - GetThunderLevel(f) * 5 / 16;
        return h * 0.8f + 0.2f;
    }
    
    
}