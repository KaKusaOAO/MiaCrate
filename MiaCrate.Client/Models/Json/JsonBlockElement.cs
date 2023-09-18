using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

public class JsonBlockElement
{
    [JsonPropertyName("from")] 
    public List<float> FromPoint { get; set; } = new();

    [JsonPropertyName("to")] 
    public List<float> ToPoint { get; set; } = new();

    [JsonPropertyName("rotation")] 
    public JsonBlockElementRotation? Rotation { get; set; }

    [JsonPropertyName("faces")] 
    public JsonBlockElementFaceCollection Faces { get; set; } = new();

    [JsonPropertyName("shade")] 
    public bool Shade { get; set; }
}