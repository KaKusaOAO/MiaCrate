using MiaCrate.Core;

namespace MiaCrate.World.Blocks;

public partial class Block
{
    private const float StoneDestroyTime = 1.5f;
    private const float StoneExplosionResistance = 6f;
    
    public static readonly Block Air = Register("air", new AirBlock(new BlockProperties
    {
        Replaceable = true,
        HasCollision = false,
        Drops = BuiltInLootTables.Empty,
        IsAir = true
    }));

    public static readonly Block Stone = Register("stone", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block Granite = Register("granite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block PolishedGranite = Register("polished_granite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block Diorite = Register("diorite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block PolishedDiorite = Register("polished_diorite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block Andesite = Register("andesite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));
    
    public static readonly Block PolishedAndesite = Register("polished_andesite", new Block(new BlockProperties
    {
        RequiresCorrectToolForDrops = true,
        DestroyTime = StoneDestroyTime,
        ExplosionResistance = StoneExplosionResistance
    }));

    public static Block Register(string name, Block block) => Registry.Register(BuiltinRegistries.Block, name, block);
}