using Mochi.World.Entities.Animals;
using Mochi.World.Entities.Monsters;
using Mochi.World.Entities.Projectiles;

namespace Mochi.World.Entities;

public static partial class EntityType
{
    public static readonly EntityType<Arrow> Arrow = Register("arrow",  
        EntityType<Arrow>.Builder.Of((t, l) => new Arrow(t, l), MobCategory.Misc));
    
    public static readonly EntityType<Chicken> Chicken = Register("chicken", 
        EntityType<Chicken>.Builder.Of((t, l) => new Chicken(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Cow> Cow = Register("cow", 
        EntityType<Cow>.Builder.Of((t, l) => new Cow(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Creeper> Creeper = Register("creeper",  
        EntityType<Creeper>.Builder.Of((t, l) => new Creeper(t, l), MobCategory.Monster));
    
    public static readonly EntityType<Horse> Horse = Register("horse",  
        EntityType<Horse>.Builder.Of((t, l) => new Horse(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Ocelot> Ocelot = Register("ocelot",  
        EntityType<Ocelot>.Builder.Of((t, l) => new Ocelot(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Pig> Pig = Register("pig", 
        EntityType<Pig>.Builder.Of((t, l) => new Pig(t, l), MobCategory.Creature)); 
    
    public static readonly EntityType<Sheep> Sheep = Register("sheep", 
        EntityType<Sheep>.Builder.Of((t, l) => new Sheep(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Wolf> Wolf = Register("wolf", 
        EntityType<Wolf>.Builder.Of((t, l) => new Wolf(t, l), MobCategory.Creature));
    
    public static readonly EntityType<Zombie> Zombie = Register("zombie", 
        EntityType<Zombie>.Builder.Of((t, l) => new Zombie(t, l), MobCategory.Monster));

    public static readonly EntityType<Player> Player = Register("player",
        EntityType<Player>.Builder.CreateNothing(MobCategory.Misc));
}