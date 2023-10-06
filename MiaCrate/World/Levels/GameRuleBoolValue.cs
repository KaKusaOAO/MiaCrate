using MiaCrate.Server;
using Mochi.Brigadier.Arguments;

namespace MiaCrate.World;

public class GameRuleBoolValue : GameRuleValue<GameRuleBoolValue>, IGameRuleValueTypeHint<bool>
{
    private readonly bool _value;

    public GameRuleBoolValue(IGameRuleType<GameRuleBoolValue> type, bool value) : base(type)
    {
        _value = value;
    }
    
    public static IGameRuleType<GameRuleBoolValue> Create(bool bl, Action<GameServer, GameRuleBoolValue> consumer)
    {
        return new GameRuleType<GameRuleBoolValue, bool>(BoolArgumentType.Bool, 
            t => new GameRuleBoolValue(t, bl),
            consumer, (visitor, key, type) => visitor.VisitBool(key, (IGameRuleType<GameRuleBoolValue>) type));
    }

    public static IGameRuleType<GameRuleBoolValue> Create(bool b) => Create(b, (_, _) => {});
}