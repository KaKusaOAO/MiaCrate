namespace MiaCrate.World.Entities;

public static partial class EntityType
{
    public static readonly EntityType<Allay> Allay = Register("allay",
        EntityType<Allay>.Builder.Of(MobCategory.Creature));

    public static readonly EntityType<Arrow> Arrow = Register("arrow",  
        EntityType<Arrow>.Builder.Of(MobCategory.Misc));
    
    public static readonly EntityType<Chicken> Chicken = Register("chicken", 
        EntityType<Chicken>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Cow> Cow = Register("cow", 
        EntityType<Cow>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Creeper> Creeper = Register("creeper",  
        EntityType<Creeper>.Builder.Of(MobCategory.Monster));
    
    public static readonly EntityType<Horse> Horse = Register("horse",  
        EntityType<Horse>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Ocelot> Ocelot = Register("ocelot",  
        EntityType<Ocelot>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Pig> Pig = Register("pig", 
        EntityType<Pig>.Builder.Of(MobCategory.Creature)); 
    
    public static readonly EntityType<Sheep> Sheep = Register("sheep", 
        EntityType<Sheep>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Wolf> Wolf = Register("wolf", 
        EntityType<Wolf>.Builder.Of(MobCategory.Creature));
    
    public static readonly EntityType<Zombie> Zombie = Register("zombie", 
        EntityType<Zombie>.Builder.Of(MobCategory.Monster));

    public static readonly EntityType<Player> Player = Register("player",
        EntityType<Player>.Builder.CreateNothing(MobCategory.Misc));
}