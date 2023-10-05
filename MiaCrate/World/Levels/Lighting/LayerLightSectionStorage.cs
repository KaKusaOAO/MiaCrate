namespace MiaCrate.World;

public abstract class LayerLightSectionStorage<T> where T : DataLayerStorageMap<T>
{
    private readonly LightLayer _layer;
    private readonly ILightChunkGetter _chunkSource;
    private readonly T _updatingSectionData;
    private T _visibleSectionData;
    private readonly Dictionary<long, byte> _sectionStates = new();

    protected LayerLightSectionStorage(LightLayer layer, ILightChunkGetter chunkSource, T updatingSectionData)
    {
        _layer = layer;
        _chunkSource = chunkSource;
        _updatingSectionData = updatingSectionData;
        _visibleSectionData = updatingSectionData.Copy();
        _visibleSectionData.DisableCache();
    }

    protected DataLayer? GetDataLayer(long l, bool bl) => GetDataLayer(bl ? _updatingSectionData : _visibleSectionData, l);

    protected DataLayer? GetDataLayer(T map, long l) => map.GetLayer(l);

    protected abstract int GetLightValue(long l);
}