using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Json;

public class ComponentConverter : JsonConverter<IComponent>
{
    public override IComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(ref reader);
        return MiaComponent.FromJson(node);
    }

    public override void Write(Utf8JsonWriter writer, IComponent value, JsonSerializerOptions options)
    {
        var node = value.ToJson();
        node.WriteTo(writer);
    }
}