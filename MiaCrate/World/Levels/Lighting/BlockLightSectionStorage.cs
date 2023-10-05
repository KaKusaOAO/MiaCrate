using MiaCrate.Core;

namespace MiaCrate.World;

public class BlockLightSectionStorage : LayerLightSectionStorage<BlockLightSectionStorage.BlockDataLayerStorageMap>
{
    public BlockLightSectionStorage(ILightChunkGetter level)
        : base(LightLayer.Block, level, new BlockDataLayerStorageMap(new Dictionary<long, DataLayer>())) {}

    protected override int GetLightValue(long l)
    {
        var m = SectionPos.BlockToSection(l);
        var layer = GetDataLayer(m, false);
        return layer?.Get(
            SectionPos.SectionRelative(BlockPos.GetX(l)),
            SectionPos.SectionRelative(BlockPos.GetY(l)),
            SectionPos.SectionRelative(BlockPos.GetZ(l))) ?? 0;
    }
    
    public sealed class BlockDataLayerStorageMap : DataLayerStorageMap<BlockDataLayerStorageMap>
    {
        public BlockDataLayerStorageMap(Dictionary<long, DataLayer> map) : base(map)
        {
        }

        public override BlockDataLayerStorageMap Copy() => new(new Dictionary<long, DataLayer>(Map));
    }
}