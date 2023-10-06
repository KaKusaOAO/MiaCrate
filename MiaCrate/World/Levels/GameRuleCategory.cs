namespace MiaCrate.World;

public sealed class GameRuleCategory
{
    public static GameRuleCategory Updates { get; } = new("gamerule.category.updates");
    public static GameRuleCategory Mobs { get; } = new("gamerule.category.mobs");
    public static GameRuleCategory Spawning { get; } = new("gamerule.category.spawning");
    public static GameRuleCategory Misc { get; } = new("gamerule.category.misc");
    
    public string DescriptionId { get; }

    private GameRuleCategory(string descriptionId)
    {
        DescriptionId = descriptionId;
    }
}