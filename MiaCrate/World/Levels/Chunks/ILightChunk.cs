using MiaCrate.Core;
using MiaCrate.World.Blocks;

namespace MiaCrate.World;

public interface ILightChunk : IBlockGetter
{
    public ChunkSkyLightSources SkyLightSources { get; }
    
    public void FindBlockLightSources(Action<BlockPos, BlockState> consumer);
}