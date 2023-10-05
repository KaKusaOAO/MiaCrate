namespace MiaCrate.World;

public class SkyLightEngine : LightEngine<SkyLightEngineStorage.SkyDataLayerStorageMap, SkyLightEngineStorage>
{
    public SkyLightEngine(ILightChunkGetter level) : this(level, new SkyLightEngineStorage(level)) {}
    
    private SkyLightEngine(ILightChunkGetter level, SkyLightEngineStorage storage)
        : base(level, storage)
    {
        throw new NotImplementedException();
    }
}