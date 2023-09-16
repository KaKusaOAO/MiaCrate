using MiaCrate.World.Blocks;

namespace MiaCrate.World.Items;

public class AirItem : Item
{
    private readonly Block _block;

    public AirItem(Block block, ItemProperties properties) : base(properties)
    {
        _block = block;
    }
}