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
        throw new NotImplementedException();
    }

    public JsonNode CreateList(IEnumerable<JsonNode> input)
    {
        throw new NotImplementedException();
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
            var key = entry.Key;
            if (key is JsonValue)
            {
                try
                {
                    var k = key.GetValue<string>();
                    output.Add(k, entry.Value);
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