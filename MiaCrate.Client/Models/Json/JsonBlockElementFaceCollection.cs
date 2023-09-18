using System.Text.Json.Serialization;
using MiaCrate.Core;

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

    public Dictionary<Direction, BlockElementFace> ToDictionary()
    {
        var dict = new Dictionary<Direction, BlockElementFace>();
        AddToDictionary(dict, Direction.Down, Down);
        AddToDictionary(dict, Direction.Up, Up);
        AddToDictionary(dict, Direction.North, North);
        AddToDictionary(dict, Direction.South, South);
        AddToDictionary(dict, Direction.West, West);
        AddToDictionary(dict, Direction.East, East);

        return dict;
    }

    private void AddToDictionary(Dictionary<Direction, BlockElementFace> dict, Direction direction, JsonBlockElementFace? face)
    {
        if (face == null) return;
        dict[direction] = new BlockElementFace(face);
    }
}