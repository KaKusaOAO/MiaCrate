using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public partial class Block
{
    private const float StoneDestroyTime = 1.5f;
    private const float StoneExplosionResistance = 6f;
    
    public static Block Air { get; } = Register("air", new AirBlock(new BlockProperties
    {
        Replaceable = true,
        HasCollision = false,
        Drops = BuiltInLootTables.Empty,
        IsAir = true
    }));

    public static Block Stone { get; } = Register("stone", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block Granite { get; } = Register("granite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block PolishedGranite { get; } = Register("polished_granite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block Diorite { get; } = Register("diorite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block PolishedDiorite { get; } = Register("polished_diorite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block Andesite { get; } = Register("andesite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static Block PolishedAndesite { get; } = Register("polished_andesite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));

    public static Block Furnace { get; } = Register("furnace", new FurnaceBlock(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = 3.5f,
        ExplosionResistance = 3.5f,
    }));

    public static Block Register(string name, Block block) => Registry.Register(BuiltinRegistries.Block, name, block);
}