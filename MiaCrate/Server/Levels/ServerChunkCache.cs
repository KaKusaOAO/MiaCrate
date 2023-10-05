using MiaCrate.Common;
using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public class ServerChunkCache : ChunkSource
{
    public override Level Level { get; }

    public override LevelLightEngine LightEngine => throw new NotImplementedException();

    public ServerChunkCache(ServerLevel level, LevelStorageAccess levelStorageAccess, IDataFixer dataFixer,
        StructureTemplateManager templateManager, IExecutor executor, ChunkGenerator generator, int i, int j, bool bl,
        IChunkProgressListener progressListener, ChunkStatusUpdateListener updateListener,
        Func<DimensionDataStorage> func)
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