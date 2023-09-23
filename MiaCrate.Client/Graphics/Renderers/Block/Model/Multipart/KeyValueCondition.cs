using MiaCrate.Extensions;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class KeyValueCondition : ICondition
{
    private readonly string _key;
    private readonly string _value;

    public KeyValueCondition(string key, string value)
    {
        _key = key;
        _value = value;
    }

    private static List<string> SplitByPipe(string str) =>
        str.Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();

    public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition)
    {
        var property = stateDefinition.GetProperty(_key);
        if (property == null)
            throw new Exception($"Unknown property '{_key}' on '{stateDefinition.Owner}'");

        var value = _value;
        var bl = !string.IsNullOrEmpty(value) && value[0] == '!';
        if (bl)
        {
            value = value[1..];
        }

        var list = SplitByPipe(value);
        if (!list.Any())
            throw new Exception($"Empty value '{_value}' for property '{_key}' on '{stateDefinition.Owner}'");

        Predicate<BlockState> predicate = list.Count == 1
            ? GetBlockStatePredicate(stateDefinition, property, value)
            : b => 
                list.Select(s => GetBlockStatePredicate(stateDefinition, property, s))
                    .Any(p => p(b));

        return bl ? predicate.Negate() : predicate;
    }

    private Predicate<BlockState> GetBlockStatePredicate(IStateDefinition<Block, BlockState> stateDefinition, IProperty property, string value)
    {
        var optional = property.GetValue(value);
        if (optional.IsEmpty)
            throw new Exception(
                $"Unknown value '{value}' for property '{_key}' on '{stateDefinition.Owner}' in '{_value}'");

        return b => b.GetValue(property).Equals(optional.Value);
    }
}