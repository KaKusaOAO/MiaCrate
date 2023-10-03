using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public abstract class BlockEntity
{
    private readonly IBlockEntityType _type;
    private readonly BlockPos _worldPosition;
    private readonly BlockState _blockState;

    protected BlockEntity(IBlockEntityType type, BlockPos worldPosition, BlockState blockState)
    {
        _type = type;
        _worldPosition = worldPosition;
        _blockState = blockState;
    }
}