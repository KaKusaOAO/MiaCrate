using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class Selector // : ICondition // which makes sense
{
    private readonly ICondition _condition;
    public MultiVariant Variant { get; }

    public Selector(ICondition condition, MultiVariant variant)
    {
        _condition = condition;
        Variant = variant;
    }

    public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition) =>
        _condition.GetPredicate(stateDefinition);

    public static Selector FromJson(JsonNode? node)
    {
        var obj = node!.AsObject();
        return new Selector(GetSelector(obj), MultiVariant.FromJson(obj["apply"]));
    }

    private static ICondition GetSelector(JsonObject obj)
    {
        if (!obj.TryGetPropertyValue("when", out var node)) return ICondition.True;
        return GetCondition(node!.AsObject());
    }

    private static ICondition GetCondition(JsonObject obj)
    {
        if (!obj.Any())
            throw new JsonException("No elements found in selector");

        if (obj.Count == 1)
        {
            if (obj.TryGetPropertyValue(OrCondition.Token, out var orNode))
            {
                var or = orNode!.AsArray().Select(n => GetCondition(n!.AsObject()));
                return new OrCondition(or);
            } 
            
            if (obj.TryGetPropertyValue(AndCondition.Token, out var andNode))
            {
                var and = andNode!.AsArray().Select(n => GetCondition(n!.AsObject()));
                return new AndCondition(and);
            }

            return GetKeyValueCondition(obj.First());
        }

        return new AndCondition(obj.Select(GetKeyValueCondition));
    }

    private static ICondition GetKeyValueCondition(KeyValuePair<string, JsonNode?> entry)
    {
        return new KeyValueCondition(entry.Key, entry.Value!.GetValue<string>());
    }
}