namespace MiaCrate.World;

public interface IGameRuleValue
{
    
}

public interface IGameRuleValueTypeHint<T>
{
    
}

public abstract class GameRuleValue<T> : IGameRuleValue where T : GameRuleValue<T>
{
    private readonly IGameRuleType<T> _type;

    public GameRuleValue(IGameRuleType<T> type)
    {
        _type = type;
    }
}