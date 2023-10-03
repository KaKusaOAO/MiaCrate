using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public abstract class BaseContainerBlockEntity : BlockEntity
{
    protected BaseContainerBlockEntity(IBlockEntityType type, BlockPos worldPosition, BlockState blockState) 
        : base(type, worldPosition, blockState)
    {
    }
}