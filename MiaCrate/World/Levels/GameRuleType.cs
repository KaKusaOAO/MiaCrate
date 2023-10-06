using MiaCrate.Commands;
using MiaCrate.Server;
using Mochi.Brigadier.Arguments;
using Mochi.Brigadier.Builder;

namespace MiaCrate.World;

public interface IGameRuleType
{
    
}

public interface IGameRuleType<T> : IGameRuleType where T : GameRuleValue<T>
{
    
}

public class GameRuleType<T, TValue> : IGameRuleType<T> where T : GameRuleValue<T>, IGameRuleValueTypeHint<TValue>
{
    private readonly Func<IArgumentType<TValue>> _argument;
    private readonly Func<GameRuleType<T, TValue>, T> _constructor;
    private readonly Action<GameServer, T> _callback;
    private readonly GameRuleVisitorCaller<T> _visitorCaller;

    public GameRuleType(Func<IArgumentType<TValue>> argument, Func<GameRuleType<T, TValue>, T> constructor, 
        Action<GameServer, T> callback, GameRuleVisitorCaller<T> visitorCaller)
    {
        _argument = argument;
        _constructor = constructor;
        _callback = callback;
        _visitorCaller = visitorCaller;
    }

    public RequiredArgumentBuilder<CommandSourceStack, TValue> CreateArgument(string str) => 
        CommandManager.Argument(str, _argument());
}