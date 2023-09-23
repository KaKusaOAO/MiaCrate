using System.Text.Json.Nodes;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public class JsonOps : IDynamicOps<JsonNode>
{
    public static readonly JsonOps Instance = new();
    
    public JsonNode Empty => null!;

    private IDynamicOps<JsonNode> Boxed() => this;

    public TOut ConvertTo<TOut>(IDynamicOps<TOut> outOps, JsonNode? input)
    {
        if (input == null) return outOps.Empty;
        if (input is JsonObject obj)
            return Boxed().ConvertMap(outOps, obj);
        if (input is JsonArray arr)
            return Boxed().ConvertList(outOps, arr);

        var value = input.AsValue();
        if (value.TryGetValue(out string? str))
            return outOps.CreateString(str);
        if (value.TryGetValue(out bool val))
            return outOps.CreateBool(val);
        if (value.TryGetValue(out byte b))
            return outOps.CreateByte(b);
        if (value.TryGetValue(out short s))
            return outOps.CreateShort(s);
        if (value.TryGetValue(out int i))
            return outOps.CreateInt(i);
        if (value.TryGetValue(out long l))
            return outOps.CreateLong(l);
        if (value.TryGetValue(out float f))
            return outOps.CreateFloat(f);
        if (value.TryGetValue(out double d))
            return outOps.CreateDouble(d);

        throw new Exception("Invalid JSON: " + input);
    }

    public JsonNode CreateString(string value) => JsonValue.Create(value)!;

    public IRecordBuilder<JsonNode> MapBuilder => new JsonRecordBuilder(this);
    public IListBuilder<JsonNode> ListBuilder => new ArrayBuilder();

    public IDataResult<IEnumerable<JsonNode>> GetEnumerable(JsonNode input)
    {
        if (input is JsonArray arr)
        {
            return DataResult.Success<IEnumerable<JsonNode>>(arr!);
        }
        
        return DataResult.Error<IEnumerable<JsonNode>>(() => $"Not a JSON array: {input}");
    }

    public IDataResult<IEnumerable<IPair<JsonNode, JsonNode>>> GetMapValues(JsonNode input)
    {
        if (input is not JsonObject obj) 
            return DataResult.Error<IEnumerable<IPair<JsonNode, JsonNode>>>(() => $"Not a JSON object: {input}");

        return DataResult.Success(obj
            .Select(e => Pair.Of((JsonNode) JsonValue.Create(e.Key)!, e.Value))
        );
    }

    public JsonNode CreateList(IEnumerable<JsonNode> input)
    {
        var result = new JsonArray();
        foreach (var node in input)
        {
            result.Add(JsonNode.Parse(node.ToJsonString()));
        }

        return result;
    }

    public JsonNode CreateMap(IEnumerable<IPair<JsonNode, JsonNode>> map)
    {
        var result = new JsonObject();
        foreach (var pair in map)
        {
            result[pair.First.GetValue<string>()] = pair.Second;
        }

        return result;
    }

    public IDataResult<JsonNode> MergeToMap(JsonNode prefix, IMapLike<JsonNode> mapLike)
    {
        var obj = (prefix as JsonObject)!;
        if (prefix is not JsonObject && prefix != Empty)
            return DataResult.Error<JsonNode>(() => $"mergeToMap called with not a map: {prefix}", prefix);

        var output = new JsonObject();
        if (prefix != Empty)
        {
            foreach (var (key, value) in obj)
            {
                output[key] = value;
            }
        }

        var missed = new List<JsonNode>();
        foreach (var entry in mapLike.Entries)
        {
            var key = entry.First!;
            if (key is JsonValue)
            {
                try
                {
                    var k = key.GetValue<string>();
                    output.Add(k, entry.Second);
                }
                catch
                {
                    missed.Add(key);
                }
            }
            else
            {
                missed.Add(key);
            }
        }

        if (missed.Any())
        {
            return DataResult.Error<JsonNode>(() => $"some keys are not strings: {missed}", output);
        }
        
        return DataResult.Success<JsonNode>(output);
    }

    public IDataResult<JsonNode> MergeToList(JsonNode list, JsonNode value)
    {
        throw new NotImplementedException();
    }

    public JsonNode CreateNumeric(decimal val) => JsonValue.Create(val);
    
    public IDataResult<string> GetStringValue(JsonNode value)
    {
        try
        {
            return DataResult.Success(value.GetValue<string>());
        }
        catch
        {
            return DataResult.Error<string>(() => $"Not a string: {value}"); 
        }
    }

    public IDataResult<decimal> GetNumberValue(JsonNode value)
    {
        try
        {
            return DataResult.Success(value.GetValue<decimal>());
        }
        catch
        {
            return DataResult.Error<decimal>(() => $"Not a number: {value}"); 
        }
    }

    public IDataResult<bool> GetBoolValue(JsonNode input)
    {
        try
        {
            return DataResult.Success(input.GetValue<bool>());
        }
        catch
        {
            return DataResult.Error<bool>(() => $"Not a boolean: {input}"); 
        }
    }
}

public class ArrayBuilder : IListBuilder<JsonNode>
{
    private IDataResult<JsonArray> _builder = DataResult.Success(new JsonArray(), Lifecycle.Stable);

    public IDynamicOps<JsonNode> Ops => JsonOps.Instance;

    public IListBuilder<JsonNode> Add(JsonNode value)
    {
        _builder = _builder.Select(b =>
        {
            b.Add(value);
            return b;
        });
        return this;
    }

    public IListBuilder<JsonNode> Add(IDataResult<JsonNode> value)
    {
        _builder = _builder.Apply2Stable((b, element) =>
        {
            b.Add(element);
            return b;
        }, value);
        return this;
    }

    public IDataResult<JsonNode> Build(JsonNode prefix)
    {
        var result = _builder.SelectMany(b =>
        {
            if (prefix is not JsonArray && prefix != null!)
            {
                return DataResult.Error(() => $"Cannot append a list to a not list: {prefix}", prefix);
            }

            var array = new JsonArray();
            if (prefix != null!)
            {
                if (prefix is not JsonArray arr)
                    throw new Exception("Prefix != null && not list");

                foreach (var node in arr)
                {
                    array.Add(node);
                }
            }

            foreach (var node in b)
            {
                array.Add(node);
            }

            return DataResult.Success<JsonNode>(array, Lifecycle.Stable);
        });
        
        _builder = DataResult.Success(new JsonArray(), Lifecycle.Stable);
        return result;
    }
}