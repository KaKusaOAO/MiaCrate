using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILightChunkGetter
{
    public IBlockGetter Level { get; }
    
    public ILightChunk? GetChunkForLighting(int x, int z);

    public void OnLightUpdate(LightLayer layer, SectionPos pos);
}