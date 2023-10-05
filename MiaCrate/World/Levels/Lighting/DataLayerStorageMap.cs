namespace MiaCrate.World;

public abstract class DataLayerStorageMap<T> where T : DataLayerStorageMap<T>
{
    private const int CacheSize = 2;
    private readonly long[] _lastSectionKeys = new long[CacheSize];
    private readonly DataLayer?[] _lastSections = new DataLayer[CacheSize];
    private bool _cacheEnabled;
    
    protected Dictionary<long, DataLayer> Map { get; }

    protected DataLayerStorageMap(Dictionary<long, DataLayer> map)
    {
        Map = map;
        ClearCache();
        _cacheEnabled = true;
    }

    public abstract T Copy();

    public DataLayer? GetLayer(long l)
    {
        throw new NotImplementedException();
    }

    public void ClearCache()
    {
        for (var i = 0; i < CacheSize; i++)
        {
            _lastSectionKeys[i] = long.MaxValue;
            _lastSections[i] = null;
        }
    }

    public void DisableCache()
    {
        _cacheEnabled = false;
    }
}