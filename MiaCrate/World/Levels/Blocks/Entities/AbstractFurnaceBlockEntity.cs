using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public abstract class AbstractFurnaceBlockEntity : BaseContainerBlockEntity
{
    protected AbstractFurnaceBlockEntity(IBlockEntityType type, BlockPos worldPosition, BlockState blockState) 
        : base(type, worldPosition, blockState)
    {
    }
}