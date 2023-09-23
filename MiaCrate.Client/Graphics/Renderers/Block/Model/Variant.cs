using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Client.Resources;

namespace MiaCrate.Client.Graphics;

public class Variant : IModelState
{
    public ResourceLocation ModelLocation { get; }
    public Transformation Rotation { get; }
    public bool IsUvLocked { get; }
    public int Weight { get; }

    public Variant(ResourceLocation modelLocation, Transformation rotation, bool uvLock, int weight)
    {
        ModelLocation = modelLocation;
        Rotation = rotation;
        IsUvLocked = uvLock;
        Weight = weight;
    }

    public static Variant FromJson(JsonNode? node)
    {
        var obj = node!.AsObject();
        var location = GetModel(obj);
        var rotation = GetBlockRotation(obj);
        var uvLock = GetUvLock(obj);
        var weight = GetWeight(obj);

        return new Variant(location, rotation.Transformation, uvLock, weight);
    }
    
    private static ResourceLocation GetModel(JsonObject obj) => new(obj["model"]!.GetValue<string>());

    private static BlockModelRotation GetBlockRotation(JsonObject obj)
    {
        var x = obj.TryGetPropertyValue("x", out var xNode) ? xNode!.GetValue<int>() : 0;
        var y = obj.TryGetPropertyValue("y", out var yNode) ? yNode!.GetValue<int>() : 0;
        return BlockModelRotation.By(x, y) ??
               throw new JsonException($"Invalid BlockModelRotation x: {x}, y: {y}");
    }

    private static bool GetUvLock(JsonObject obj) =>
        obj.TryGetPropertyValue("uvlock", out var node) && node!.GetValue<bool>();

    private static int GetWeight(JsonObject obj)
    {
        var i = obj.TryGetPropertyValue("weight", out var node) ? node!.GetValue<int>() : 1;
        if (i < 1)
            throw new JsonException($"Invalid weight {i} found, expected integer >= 1");

        return i;
    }
}