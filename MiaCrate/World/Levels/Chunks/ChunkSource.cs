using MiaCrate.Core;

namespace MiaCrate.World;

public abstract class ChunkSource : ILightChunkGetter, IDisposable
{
    public abstract IBlockGetter Level { get; }
    public abstract LevelLightEngine LightEngine { get; }

    public LevelChunk? GetChunk(int x, int z, bool bl) => GetChunk(x, z, ChunkStatus.Full, bl);
    public LevelChunk? GetChunkNow(int x, int z) => GetChunk(x, z, false);
    public virtual LevelChunk? GetChunkForLighting(int x, int z) => GetChunk(x, z, ChunkStatus.Empty, false);
    ILightChunk? ILightChunkGetter.GetChunkForLighting(int x, int z) => GetChunkForLighting(x, z);

    public virtual bool HasChunk(int x, int z) => GetChunk(x, z, ChunkStatus.Full, false) != null;

    public abstract LevelChunk? GetChunk(int x, int z, ChunkStatus status, bool bl);

    public abstract void OnLightUpdate(LightLayer layer, SectionPos pos);

    public virtual void Dispose()
    {
        
    }
}

public class LevelChunk : ChunkAccess
{
    
}