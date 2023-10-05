using MiaCrate.Client.Graphics;
using MiaCrate.Client.Players;
using MiaCrate.Client.Utils;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World;
using MiaCrate.World.Dimensions;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Multiplayer;

public class ClientLevel : Level
{
    private const double FluidParticleSpawnOffset = 0.05;
    private const int NormalLightUpdatesPerFrame = 10;
    private const int LightUpdateQueueSizeThreshold = 1000;

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    private static Argb32 CloudColor { get; } = Argb32.White;

    public ClientPacketListener Connection { get; }
    public LevelRenderer LevelRenderer { get; }
    
    public override ChunkSource ChunkSource => throw new NotImplementedException();
    public override List<Player> Players => throw new NotImplementedException();
    
    public ClientLevel(ClientPacketListener connection, ClientLevelData levelData, IResourceKey<Level> dimension,
        IHolder<DimensionType> dimensionTypeRegistration, int i, int j, Func<IProfilerFiller> profiler,
        LevelRenderer levelRenderer, bool isDebug, long l) 
        : base(levelData, dimension, connection.RegistryAccess, dimensionTypeRegistration, profiler, true, isDebug, l, SharedConstants.MaxChainedNeighborUpdates)
    {
        Connection = connection;
        LevelRenderer = levelRenderer;
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