using System.Text.Json;
using System.Text.Json.Serialization;
using MiaCrate.Core;

namespace MiaCrate.Json;

public class DirectionConverter : JsonConverter<Direction>
{
    public override Direction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
        Direction.Codec.ByName(reader.GetString());

    public override void Write(Utf8JsonWriter writer, Direction value, JsonSerializerOptions options) => 
        writer.WriteStringValue(value.SerializedName);
}