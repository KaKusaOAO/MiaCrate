using MiaCrate.Core;
using MiaCrate.World.Blocks;

namespace MiaCrate.World;

public abstract class ChunkAccess : IBlockGetter, INoiseBiomeSource, ILightChunk, IStructureAccess
{
    public const int NoFilledSection = -1;
    
    public int Height => throw new NotImplementedException();

    public int MinBuildHeight => throw new NotImplementedException();

    public BlockEntity? GetBlockEntity(BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public BlockState GetBlockState(BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public IHolder<Biome> GetNoiseBiome(int x, int y, int z)
    {
        throw new NotImplementedException();
    }

    public ChunkSkyLightSources SkyLightSources => throw new NotImplementedException();

    public void FindBlockLightSources(Action<BlockPos, BlockState> consumer)
    {
        throw new NotImplementedException();
    }

    public StructureStart? GetStartForStructure(Structure structure)
    {
        throw new NotImplementedException();
    }

    public void SetStartForStructure(Structure structure, StructureStart start)
    {
        throw new NotImplementedException();
    }
}