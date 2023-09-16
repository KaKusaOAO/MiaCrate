using MiaCrate.Core;
using MiaCrate.World.Items;

namespace MiaCrate.World.Blocks;

public partial class Block : BlockBehavior, IItemLike
{
    public const float Indestructible = -1f;
    public const float Instant = 0f;
    public const int UpdateLimit = 512;
    public const int CacheSize = 2048;

    private readonly IReferenceHolder<Block> _builtInRegistryHolder;

    
    public Block(BlockProperties properties) : base(properties)
    {
        _builtInRegistryHolder = BuiltinRegistries.Block.CreateIntrusiveHolder(this);
    }

    public Item AsItem()
    {
        throw new NotImplementedException();
    }
}