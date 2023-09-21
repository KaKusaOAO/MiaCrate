using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class BlockModelDefinition
{
    private readonly Dictionary<string, MultiVariant> _variants;
    private readonly MultiPart? _multiPart;

    private BlockModelDefinition(Dictionary<string, MultiVariant> variants, MultiPart? multiPart)
    {
        _variants = variants;
        _multiPart = multiPart;
    }

    public static BlockModelDefinition FromStream(Context context, Stream stream)
    {
        var node = JsonNode.Parse(stream);
        return FromJson(context, node);
    }

    public static BlockModelDefinition FromJson(Context context, JsonNode? node)
    {
        var obj = node!.AsObject();
        var dict = GetVariants(obj);
        var multipart = GetMultiPart(context, obj);

        if (dict.Any() || multipart != null && multipart.MultiVariants.Any())
            return new BlockModelDefinition(dict, multipart);

        throw new JsonException("Neither 'variants' nor 'multipart' found");
    }

    private static MultiPart? GetMultiPart(Context context, JsonObject obj)
    {
        if (!obj.TryGetPropertyValue("multipart", out var node)) return null;

        var arr = node!.AsArray();
        return MultiPart.FromJson(context, arr);
    }

    private static Dictionary<string, MultiVariant> GetVariants(JsonObject obj)
    {
        var dict = new Dictionary<string, MultiVariant>();
        if (!obj.TryGetPropertyValue("variants", out var variantsNode))
            return dict;

        var variants = variantsNode!.AsObject();
        foreach (var (key, value) in variants)
        {
            dict[key] = MultiVariant.FromJson(value);
        }

        return dict;
    }

    public class Context
    {
        // Every context used to have an individual Gson instance,
        // but those instances don't use any of values from the context (???)

        public IStateDefinition<Block, BlockState> Definition { get; set; }
    }   
}