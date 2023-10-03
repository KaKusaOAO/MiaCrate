using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public class FurnaceBlock : AbstractFurnaceBlock
{
    public FurnaceBlock(BlockProperties properties) : base(properties)
    {
    }

    public override BlockEntity? NewBlockEntity(BlockPos pos, BlockState state)
    {
        return new FurnaceBlockEntity(pos, state);
    }
}