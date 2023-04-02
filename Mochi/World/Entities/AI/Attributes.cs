using Mochi.Core;

namespace Mochi.World.Entities.AI;

public static class Attributes
{
    public static readonly Attribute MaxHealth = Register("generic.max_health", new RangedAttribute("attribute.name.generic.max_health", 20.0, 1.0, 1024.0).SetSyncable(true));
    public static readonly Attribute FollowRange = Register("generic.follow_range", new RangedAttribute("attribute.name.generic.follow_range", 32.0, 0.0, 2048.0));
    public static readonly Attribute KnockbackResistance = Register("generic.knockback_resistance", new RangedAttribute("attribute.name.generic.knockback_resistance", 0.0, 0.0, 1.0));
    public static readonly Attribute MovementSpeed = Register("generic.movement_speed", new RangedAttribute("attribute.name.generic.movement_speed", 0.699999988079071, 0.0, 1024.0).SetSyncable(true));
    public static readonly Attribute FlyingSpeed = Register("generic.flying_speed", new RangedAttribute("attribute.name.generic.flying_speed", 0.4000000059604645, 0.0, 1024.0).SetSyncable(true));
    public static readonly Attribute AttackDamage = Register("generic.attack_damage", new RangedAttribute("attribute.name.generic.attack_damage", 2.0, 0.0, 2048.0));
    public static readonly Attribute AttackKnockback = Register("generic.attack_knockback", new RangedAttribute("attribute.name.generic.attack_knockback", 0.0, 0.0, 5.0));
    public static readonly Attribute AttackSpeed = Register("generic.attack_speed", new RangedAttribute("attribute.name.generic.attack_speed", 4.0, 0.0, 1024.0).SetSyncable(true));
    public static readonly Attribute Armor = Register("generic.armor", new RangedAttribute("attribute.name.generic.armor", 0.0, 0.0, 30.0).SetSyncable(true));
    public static readonly Attribute ArmorToughness = Register("generic.armor_toughness", new RangedAttribute("attribute.name.generic.armor_toughness", 0.0, 0.0, 20.0).SetSyncable(true));
    public static readonly Attribute Luck = Register("generic.luck", new RangedAttribute("attribute.name.generic.luck", 0.0, -1024.0, 1024.0).SetSyncable(true));
    public static readonly Attribute SpawnReinforcementsChance = Register("zombie.spawn_reinforcements", new RangedAttribute("attribute.name.zombie.spawn_reinforcements", 0.0, 0.0, 1.0));
    public static readonly Attribute JumpStrength = Register("horse.jump_strength", new RangedAttribute("attribute.name.horse.jump_strength", 0.7, 0.0, 2.0).SetSyncable(true));
    
    private static Attribute Register(string name, Attribute attribute) => 
        Registry.Register(Registry.Attribute, name, attribute);
}