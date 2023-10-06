namespace MiaCrate.World;

public interface IGameRuleKey
{
    
}

public class GameRuleKey<T> : IGameRuleKey where T : GameRuleValue<T>
{
    public string Id { get; }
    public GameRuleCategory Category { get; }

    public GameRuleKey(string id, GameRuleCategory category)
    {
        Id = id;
        Category = category;
    }
}