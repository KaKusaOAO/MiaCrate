using System.Text.Json.Serialization;

namespace MiaCrate.Client.Models.Json;

public class JsonItemTransforms
{
    [JsonPropertyName("thirdperson_lefthand")]
    public JsonItemTransform? ThirdPersonLeftHand { get; set; }
    
    [JsonPropertyName("thirdperson_righthand")]
    public JsonItemTransform? ThirdPersonRightHand { get; set; }
    
    [JsonPropertyName("firstperson_lefthand")]
    public JsonItemTransform? FirstPersonLeftHand { get; set; }
    
    [JsonPropertyName("firstperson_righthand")]
    public JsonItemTransform? FirstPersonRightHand { get; set; }
    
    [JsonPropertyName("head")]
    public JsonItemTransform? Head { get; set; }
    
    [JsonPropertyName("gui")]
    public JsonItemTransform? Gui { get; set; }
    
    [JsonPropertyName("ground")]
    public JsonItemTransform? Ground { get; set; }
    
    [JsonPropertyName("fixed")]
    public JsonItemTransform? Fixed { get; set; }
}