using MiaCrate.Data.Codecs;

namespace MiaCrate.World.Blocks;

public class BlockState : BlockBehavior.BlockStateBase
{
    public BlockState(Block owner, Dictionary<IProperty, IComparable> values, IMapCodec<BlockState> propertiesCodec) 
        : base(owner, values, propertiesCodec)
    {
    }

    public static StateDefinition<Block, BlockState>.Factory CreateFactory() =>
        (owner, map, codec) => new BlockState(owner, map, codec);

    protected override BlockState AsState() => this;
}