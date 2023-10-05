using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public abstract class BlockEntity
{
    private readonly BlockPos _worldPosition;
    private readonly BlockState _blockState;

    public IBlockEntityType Type { get; }
    
    protected BlockEntity(IBlockEntityType type, BlockPos worldPosition, BlockState blockState)
    {
        Type = type;
        _worldPosition = worldPosition;
        _blockState = blockState;
    }
}