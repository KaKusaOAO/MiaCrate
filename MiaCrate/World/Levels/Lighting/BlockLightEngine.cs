namespace MiaCrate.World;

public sealed class BlockLightEngine : LightEngine<BlockLightSectionStorage.BlockDataLayerStorageMap, BlockLightSectionStorage>
{
    public BlockLightEngine(ILightChunkGetter chunkSource) 
        : base(chunkSource, new BlockLightSectionStorage(chunkSource)) { }
}