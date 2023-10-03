namespace MiaCrate.World.Blocks;

public static partial class BlockEntityType
{
    public static BlockEntityType<FurnaceBlockEntity> Furnace { get; } =
        Register<FurnaceBlockEntity>("furnace", BlockEntityType<FurnaceBlockEntity>.Builder.Of(Block.Furnace));
}