using System.Text.Json.Serialization;
using MiaCrate.Client.Graphics;
using MiaCrate.Core;
using MiaCrate.Json;

namespace MiaCrate.Client.Models.Json;

public class JsonBlockElementFace
{
    [JsonPropertyName("cullface")]
    [JsonConverter(typeof(DirectionConverter))]
    public Direction? CullFace { get; set; }

    [JsonPropertyName("tintindex")] 
    public int TintIndex { get; set; } = BlockElementFace.NoTint;

    [JsonPropertyName("texture")] 
    public string Texture { get; set; }

    [JsonPropertyName("uv")]
    public List<float>? Uv { get; set; }
    
    [JsonPropertyName("rotation")]
    public int Rotation { get; set; }
}