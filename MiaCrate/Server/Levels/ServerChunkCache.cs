using MiaCrate.Core;
using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public class ServerChunkCache : ChunkSource
{
    public override Level Level { get; }

    public override LevelLightEngine LightEngine => throw new NotImplementedException();

    public ServerChunkCache(ServerLevel level)
    {
        Level = level;
    }

    public override LevelChunk? GetChunk(int x, int z, ChunkStatus status, bool bl)
    {
        throw new NotImplementedException();
    }

    public override void OnLightUpdate(LightLayer layer, SectionPos pos)
    {
        throw new NotImplementedException();
    }
}