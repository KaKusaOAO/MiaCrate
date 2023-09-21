using System.Collections;
using System.Text.Json.Nodes;

namespace MiaCrate.Auth;

public class PropertyMap : Multimap<string, Property>
{
    public static PropertyMap FromJson(JsonNode? node)
    {
        var result = new PropertyMap();
        if (node == null) return result;

        if (node is JsonObject obj)
        {
            foreach (var (key, value) in obj)
            {
                if (value is JsonArray arr)
                {
                    foreach (var element in arr)
                    {
                        result.Add(key, new Property(key, element!.GetValue<string>()));
                    }
                }   
            }
        } else if (node is JsonArray arr)
        {
            foreach (var element in arr)
            {
                if (element is JsonObject item)
                {
                    var name = item["name"]!.GetValue<string>();
                    var val = item["value"]!.GetValue<string>();
                    result.Add(name, new Property(name, val, item["signature"]?.GetValue<string>()));
                }
            }
        }

        return result;
    }

    public JsonNode ToJson()
    {
        var result = new JsonArray();
        foreach (var property in Values.SelectMany(v => v))
        {
            var obj = new JsonObject
            {
                ["name"] = property.Name,
                ["value"] = property.Value
            };

            if (property.HasSignature)
            {
                obj["signature"] = property.Signature;
            }
            
            result.Add((JsonNode) obj);
        }

        return result;
    }
}