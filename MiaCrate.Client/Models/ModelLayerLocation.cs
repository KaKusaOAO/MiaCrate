namespace MiaCrate.Client.Models;

public sealed class ModelLayerLocation
{
    public ResourceLocation Model { get; }
    public string Layer { get; }
    
    public ModelLayerLocation(ResourceLocation model, string layer)
    {
        Model = model;
        Layer = layer;
    }

    public override int GetHashCode() => HashCode.Combine(Model, Layer);
}