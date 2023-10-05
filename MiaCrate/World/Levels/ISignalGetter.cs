using MiaCrate.Core;

namespace MiaCrate.World;

public interface ISignalGetter : IBlockGetter
{
    public int GetDirectSignal(BlockPos pos, Direction direction) =>
        GetBlockState(pos).GetDirectSignal(this, pos, direction);

    public int GetDirectSignalTo(BlockPos pos)
    {
        var i = 0;

        foreach (var direction in Direction.Values)
        {
            i = Math.Max(i, GetDirectSignal(pos.Relative(direction), direction));
            if (i >= Redstone.SignalMax) return i;
        }

        return i;
    }
}