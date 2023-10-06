namespace MiaCrate.World;

public class GameRules
{
    public const int DefaultRandomTickSpeed = 3;
    
    private static readonly SortedDictionary<IGameRuleKey, IGameRuleType> _gameRuleTypes = new();

    public static GameRuleKey<GameRuleBoolValue> RuleDoFireTick { get; } = 
        Register("doFireTick", GameRuleCategory.Updates, GameRuleBoolValue.Create(true));
    
    public static GameRuleKey<GameRuleBoolValue> RuleMobGriefing { get; } = 
        Register("mobGriefing", GameRuleCategory.Mobs, GameRuleBoolValue.Create(true));

    public static GameRuleKey<GameRuleIntValue> RuleRandomTicking { get; } =
        Register("randomTickSpeed", GameRuleCategory.Updates, GameRuleIntValue.Create(DefaultRandomTickSpeed));

    public static GameRuleKey<GameRuleIntValue> RuleMaxEntityCramming { get; } =
        Register("maxEntityCramming", GameRuleCategory.Mobs, GameRuleIntValue.Create(24));
    
    public static GameRuleKey<GameRuleIntValue> RuleMaxCommandChainLength { get; } =
        Register("maxCommandChainLength", GameRuleCategory.Mobs, GameRuleIntValue.Create(65536));

    public static GameRuleKey<GameRuleBoolValue> RuleDoInsomnia { get; } =
        Register("doInsomnia", GameRuleCategory.Spawning, GameRuleBoolValue.Create(true));

    private static GameRuleKey<T> Register<T>(string name, GameRuleCategory category, IGameRuleType<T> type)
        where T : GameRuleValue<T>
    {
        var key = new GameRuleKey<T>(name, category);
        if (!_gameRuleTypes.TryAdd(key, type))
            throw new InvalidOperationException($"Duplicate game rule registration for {name}");

        return key;
    }
}