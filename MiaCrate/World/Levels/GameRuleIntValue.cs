using MiaCrate.Server;
using Mochi.Brigadier.Arguments;

namespace MiaCrate.World;

public class GameRuleIntValue : GameRuleValue<GameRuleIntValue>, IGameRuleValueTypeHint<int>
{
    private readonly int _value;

    public GameRuleIntValue(IGameRuleType<GameRuleIntValue> type, int value) : base(type)
    {
        _value = value;
    }
    
    public static IGameRuleType<GameRuleIntValue> Create(int bl, Action<GameServer, GameRuleIntValue> consumer)
    {
        return new GameRuleType<GameRuleIntValue, int>(() => IntegerArgumentType.Integer(), 
            t => new GameRuleIntValue(t, bl),
            consumer, (visitor, key, type) => visitor.VisitInt(key, (IGameRuleType<GameRuleIntValue>) type));
    }

    public static IGameRuleType<GameRuleIntValue> Create(int b) => Create(b, (_, _) => {});
}