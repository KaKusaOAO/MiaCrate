namespace Mochi.World;

public class MobCategory
{
    public static readonly MobCategory Monster = new("monster", 70, false, false, 128);
    public static readonly MobCategory Creature = new("creature", 10, true, true, 128);
    public static readonly MobCategory Ambient = new("ambient", 15, true, false, 128);
    public static readonly MobCategory Axolotls = new("axolotls", 5, true, false, 128);
    public static readonly MobCategory UndergroundWaterCreature = 
        new("underground_water_creature", 5, true, false, 128);
    public static readonly MobCategory WaterCreature = new("water_creature", 5, true, true, 128);
    public static readonly MobCategory WaterAmbient = new("water_ambient", 20, true, false, 64);
    public static readonly MobCategory Misc = new("misc", -1, true, true, 128);

    public string Name { get; }
    public int Max { get; }
    public bool IsFriendly { get; }
    public bool IsPersistent { get; }
    public int DespawnDistance { get; }

    private MobCategory(string name, int max, bool friendly, bool persistent, int despawnDistance)
    {
        Name = name;
        Max = max;
        IsFriendly = friendly;
        IsPersistent = persistent;
        DespawnDistance = despawnDistance;
    }
}