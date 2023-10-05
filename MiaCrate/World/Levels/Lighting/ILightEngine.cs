using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILightEngine : ILayerLightEventListener
{
    public const int MaxLevel = 15;
}

public interface ILightEngineLeft<T> : ILightEngine where T : DataLayerStorageMap<T>
{
    
}

public interface ILightEngine<TM, TS> : ILightEngineLeft<TM> 
    where TM : DataLayerStorageMap<TM>
    where TS : LayerLightSectionStorage<TM>
{
    
}

public abstract class LightEngine<TM, TS> : ILightEngine<TM, TS>
    where TM : DataLayerStorageMap<TM>
    where TS : LayerLightSectionStorage<TM>
{
    protected const int MinOpacity = 1;
    private const int MinQueueSize = 512;
    private readonly ILightChunkGetter _chunkSource;
    private readonly TS _storage;
    
    private const int CacheSize = 2;
    private readonly long[] _lastChunkPos = new long[CacheSize];
    private readonly ILightChunk[] _lastChunk = new ILightChunk[CacheSize];

    public bool HasLightWork => throw new NotImplementedException();

    protected LightEngine(ILightChunkGetter chunkSource, TS storage)
    {
        _chunkSource = chunkSource;
        _storage = storage;
        ClearChunkCache();
    }

    private void ClearChunkCache()
    {
        Array.Fill(_lastChunkPos, ChunkPos.InvalidChunkPos);
        Array.Fill(_lastChunk, null);
    }

    public void CheckBlock(BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public int RunLightUpdates()
    {
        throw new NotImplementedException();
    }

    public void UpdateSectionStatus(SectionPos pos, bool bl)
    {
        throw new NotImplementedException();
    }

    public DataLayer? GetDataLayerData(SectionPos pos)
    {
        throw new NotImplementedException();
    }

    public int GetLightValue(BlockPos pos)
    {
        throw new NotImplementedException();
    }
}