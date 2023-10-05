namespace MiaCrate.World;

public class SkyLightEngineStorage : LayerLightSectionStorage<SkyLightEngineStorage.SkyDataLayerStorageMap>
{
    public SkyLightEngineStorage(ILightChunkGetter chunkSource) 
        : base(LightLayer.Sky, chunkSource, new SkyDataLayerStorageMap(
            new Dictionary<long, DataLayer>(), new Dictionary<long, int>(), int.MaxValue)) { }

    protected override int GetLightValue(long l)
    {
        throw new NotImplementedException();
    }
    
    public sealed class SkyDataLayerStorageMap : DataLayerStorageMap<SkyDataLayerStorageMap>
    {
        private readonly Dictionary<long, int> _topSections;
        private int _currentLowestY;

        public SkyDataLayerStorageMap(Dictionary<long, DataLayer> map, Dictionary<long, int> topSections, int currentLowestY) : base(map)
        {
            _topSections = topSections;
            _currentLowestY = currentLowestY;
        }

        public override SkyDataLayerStorageMap Copy()
        {
            return new SkyDataLayerStorageMap(
                new Dictionary<long, DataLayer>(Map),
                new Dictionary<long, int>(_topSections),
                _currentLowestY);
        }
    }
}