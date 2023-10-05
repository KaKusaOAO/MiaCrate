using MiaCrate.Core;
using MiaCrate.World.Blocks;
using Mochi.Utils;

namespace MiaCrate.World;

public interface ILevelSimulatedReader
{
    public bool IsStateAtPosition(BlockPos pos, Predicate<BlockState> predicate);
    public IOptional<T> GetBlockEntity<T>(BlockPos pos, IBlockEntityType<T> type) where T : BlockEntity;
}