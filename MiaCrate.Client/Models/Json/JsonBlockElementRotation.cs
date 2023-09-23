using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

public class JsonBlockElementRotation
{
    [JsonPropertyName("origin")] 
    public List<float> Origin { get; set; } = new();

    [JsonPropertyName("axis")]
    public string Axis { get; set; }

    [JsonPropertyName("angle")] 
    public float Angle { get; set; }

    [JsonPropertyName("rescale")] 
    public bool Rescale { get; set; }
}