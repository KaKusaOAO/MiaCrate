using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World.Blocks;

namespace MiaCrate.World.Items;

public partial class Item
{
    public static readonly Dictionary<Block, Item> ByBlock = new();

    public static readonly Item Air = RegisterBlock(Block.Air, new AirItem(Block.Air, new ItemProperties()));
    public static readonly Item Stone = RegisterBlock(Block.Stone);
    public static readonly Item Granite = RegisterBlock(Block.Granite);
    public static readonly Item PolishedGranite = RegisterBlock(Block.PolishedGranite);
    public static readonly Item Diorite = RegisterBlock(Block.Diorite);
    public static readonly Item PolishedDiorite = RegisterBlock(Block.PolishedDiorite);
    public static readonly Item Andesite = RegisterBlock(Block.Andesite);
    public static readonly Item PolishedAndesite = RegisterBlock(Block.PolishedAndesite);

    public static Item RegisterBlock(Block block) => RegisterBlock(new BlockItem(block, new ItemProperties()));

    public static Item RegisterBlock(BlockItem item) => RegisterBlock(item.Block, item);

    public static Item RegisterBlock(Block block, Item item) => 
        RegisterItem(BuiltinRegistries.Block.GetKey(block)!, item);

    public static Item RegisterItem(ResourceLocation location, Item item) => 
        RegisterItem(ResourceKey.Create<Item>(BuiltinRegistries.Item.Key, location), item);

    public static Item RegisterItem(IResourceKey<Item> key, Item item)
    {
        if (item is BlockItem blockItem)
        {
            blockItem.RegisterBlocks(ByBlock, item);
        }

        return Registry.Register(BuiltinRegistries.Item, key, item);
    }
}