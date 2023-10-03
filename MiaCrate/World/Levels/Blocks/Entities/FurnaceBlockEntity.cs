using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public class FurnaceBlockEntity : AbstractFurnaceBlockEntity
{
    public FurnaceBlockEntity(BlockPos worldPosition, BlockState blockState) 
        : base(BlockEntityType.Furnace, worldPosition, blockState)
    {
    }
}