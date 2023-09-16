using MiaCrate.World.Blocks;

namespace MiaCrate.World.Items;

public class BlockItem : Item
{
    public Block Block { get; }

    public BlockItem(Block block, ItemProperties properties) : base(properties)
    {
        Block = block;
    }
    
    public void RegisterBlocks(Dictionary<Block, Item> map, Item item)
    {
        map[Block] = item; // ?
    }
}