using System.Text.Json.Nodes;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public class JsonRecordBuilder : AbstractStringBuilder<JsonNode, JsonObject>
{
    public JsonRecordBuilder(JsonOps ops, Func<JsonObject>? builder = null) : base(ops, builder)
    {
    }

    protected override IDataResult<JsonNode> Build(JsonObject builder, JsonNode? prefix)
    {
        if (prefix == null) 
            return DataResult.Success<JsonNode>(builder);

        if (prefix is JsonObject obj)
        {
            var result = new JsonObject();
            foreach (var (key, value) in obj)
            {
                result[key] = value;
            }

            foreach (var (key, value) in builder)
            {
                result[key] = value;
            }

            return DataResult.Success<JsonNode>(result);
        }
        
        return DataResult.Error(() => $"mergeToMap called with not a map: {prefix}", prefix);
    }

    protected override JsonObject InitBuilder() => new();

    protected override JsonObject Append(string key, JsonNode value, JsonObject builder)
    {
        builder[key] = value;
        return builder;
    }
}