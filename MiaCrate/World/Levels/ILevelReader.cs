using MiaCrate.World.Dimensions;

namespace MiaCrate.World;

public interface ILevelReader : IBlockAndTintGetter, ICollisionGetter, ISignalGetter, INoiseBiomeSource
{
    public DimensionType DimensionType { get; }
    
    public ChunkAccess GetChunk(int x, int z, ChunkStatus status, bool bl);
}