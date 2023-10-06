using MiaCrate.Core;
using MiaCrate.World;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Colors;

public class BlockColors
{
    public static BlockColors CreateDefault()
    {
        Util.LogFoobar();
        return new BlockColors();
    }
}

public delegate int BlockColorDelegate(BlockState state, IBlockAndTintGetter? level, BlockPos? pos, int i);