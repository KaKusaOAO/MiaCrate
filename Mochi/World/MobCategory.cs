namespace Mochi.World;

public class MobCategory
{
    public static readonly MobCategory Monster = new("monster", 70, false, false, 128);
    public static readonly MobCategory Creature = new("creature", 10, true, true, 128);
    
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