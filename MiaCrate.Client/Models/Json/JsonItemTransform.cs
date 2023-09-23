using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

public class JsonItemTransform
{
    [JsonPropertyName("rotation")] 
    public List<float>? Rotation { get; set; }

    [JsonPropertyName("translation")] 
    public List<float>? Translation { get; set; }

    [JsonPropertyName("scale")] 
    public List<float>? Scale { get; set; }
}