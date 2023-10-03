using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public abstract class BaseEntityBlock : Block, IEntityBlock
{
    protected BaseEntityBlock(BlockProperties properties) : base(properties)
    {
    }

    public abstract BlockEntity? NewBlockEntity(BlockPos pos, BlockState state);
}