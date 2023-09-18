using MiaCrate.Data.Codecs;

namespace MiaCrate.World.Blocks;

public class BlockState : BlockBehavior.BlockStateBase
{
    public BlockState(Block owner, Dictionary<IProperty, IComparable> values, IMapCodec<BlockState> propertiesCodec) 
        : base(owner, values, propertiesCodec)
    {
    }
    
    protected override BlockState AsState() => this;
}