using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MiaCrate.Auth;

namespace MiaCrate.Json;

public class PropertyMapConverter : JsonConverter<PropertyMap>
{
    public override PropertyMap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonSerializer.Deserialize<JsonNode>(ref reader);
        return PropertyMap.FromJson(node);
    }

    public override void Write(Utf8JsonWriter writer, PropertyMap value, JsonSerializerOptions options)
    {
        var node = value.ToJson();
        JsonSerializer.Serialize(writer, node);
    }
}