using System.Text.Json.Serialization;
using MiaCrate.Core;
using MiaCrate.Json;

namespace MiaCrate.Client.Models;

public class JsonBlockElementFace
{
    [JsonPropertyName("cullface")]
    [JsonConverter(typeof(DirectionConverter))]
    public Direction? CullFace { get; set; }

    [JsonPropertyName("tintindex")] 
    public int TintIndex { get; set; } = -1;

    [JsonPropertyName("texture")] 
    public string Texture { get; set; }
}