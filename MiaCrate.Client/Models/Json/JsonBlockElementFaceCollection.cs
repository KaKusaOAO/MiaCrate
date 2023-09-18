using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models;

public class JsonBlockElementFaceCollection
{
    [JsonPropertyName("down")] 
    public JsonBlockElementFace? Down { get; set; }

    [JsonPropertyName("up")] 
    public JsonBlockElementFace? Up { get; set; }

    [JsonPropertyName("north")] 
    public JsonBlockElementFace? North { get; set; }

    [JsonPropertyName("south")] 
    public JsonBlockElementFace? South { get; set; }

    [JsonPropertyName("west")] 
    public JsonBlockElementFace? West { get; set; }

    [JsonPropertyName("east")] 
    public JsonBlockElementFace? East { get; set; }
}