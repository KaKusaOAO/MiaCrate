using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public interface IEntityBlock
{
    public BlockEntity? NewBlockEntity(BlockPos pos, BlockState state);
}