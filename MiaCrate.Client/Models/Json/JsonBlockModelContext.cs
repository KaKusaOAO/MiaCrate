using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonBlockModel))]
internal partial class JsonBlockModelContext : JsonSerializerContext
{
    
}