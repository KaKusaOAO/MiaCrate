using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

public class JsonBlockModel
{
    [JsonPropertyName("parent")] 
    public string Parent { get; set; } = "";

    [JsonPropertyName("elements")] 
    public List<JsonBlockElement> Elements { get; set; } = new();

    [JsonPropertyName("textures")]
    public Dictionary<string, string> Textures { get; set; } = new();

    [JsonPropertyName("ambientocclusion")] 
    public bool? AmbientOcclusion { get; set; }

    [JsonPropertyName("display")]
    public JsonItemTransforms Display { get; set; } = new();
    
    [JsonPropertyName("gui_light")]
    public string GuiLight { get; set; }
}