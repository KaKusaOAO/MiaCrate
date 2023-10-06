using MiaCrate.Core;

namespace MiaCrate.World.Entities.AI;

public static class Attributes
{
    public static Attribute MaxHealth { get; } = Register("generic.max_health",
        new RangedAttribute("attribute.name.generic.max_health", 20.0, 1.0, 1024.0).SetSyncable(true));
    
    public static Attribute FollowRange { get; } = Register("generic.follow_range",
        new RangedAttribute("attribute.name.generic.follow_range", 32.0, 0.0, 2048.0));
    
    public static Attribute KnockbackResistance { get; } = Register("generic.knockback_resistance", 
        new RangedAttribute("attribute.name.generic.knockback_resistance", 0.0, 0.0, 1.0));
    
    public static Attribute MovementSpeed { get; } = Register("generic.movement_speed", 
        new RangedAttribute("attribute.name.generic.movement_speed", 0.699999988079071, 0.0, 1024.0).SetSyncable(true));
    
    public static Attribute FlyingSpeed { get; } = Register("generic.flying_speed", 
        new RangedAttribute("attribute.name.generic.flying_speed", 0.4000000059604645, 0.0, 1024.0).SetSyncable(true));
    
    public static Attribute AttackDamage { get; } = Register("generic.attack_damage", 
        new RangedAttribute("attribute.name.generic.attack_damage", 2.0, 0.0, 2048.0));
    
    public static Attribute AttackKnockback { get; } = Register("generic.attack_knockback", 
        new RangedAttribute("attribute.name.generic.attack_knockback", 0.0, 0.0, 5.0));
    
    public static Attribute AttackSpeed { get; } = Register("generic.attack_speed", 
        new RangedAttribute("attribute.name.generic.attack_speed", 4.0, 0.0, 1024.0).SetSyncable(true));
    
    public static Attribute Armor { get; } = Register("generic.armor", 
        new RangedAttribute("attribute.name.generic.armor", 0.0, 0.0, 30.0).SetSyncable(true));
    
    public static Attribute ArmorToughness { get; } = Register("generic.armor_toughness", 
        new RangedAttribute("attribute.name.generic.armor_toughness", 0.0, 0.0, 20.0).SetSyncable(true));
    
    public static Attribute Luck { get; } = Register("generic.luck", 
        new RangedAttribute("attribute.name.generic.luck", 0.0, -1024.0, 1024.0).SetSyncable(true));

    public static Attribute MaxAbsorption { get; } = Register("generic.max_absorption",
        new RangedAttribute("attrribute.name.generic.max_absorption", 0, 0, 2048).SetSyncable(true));
    
    public static Attribute SpawnReinforcementsChance { get; } = Register("zombie.spawn_reinforcements", 
        new RangedAttribute("attribute.name.zombie.spawn_reinforcements", 0.0, 0.0, 1.0));
    
    public static Attribute JumpStrength { get; } = Register("horse.jump_strength", 
        new RangedAttribute("attribute.name.horse.jump_strength", 0.7, 0.0, 2.0).SetSyncable(true));
    
    private static Attribute Register(string name, Attribute attribute) => 
        Registry.Register(BuiltinRegistries.Attribute, name, attribute);
}