using MiaCrate.Resources;

namespace MiaCrate.Client.Models;

public class EntityModelSet : IResourceManagerReloadListener
{
    private Dictionary<ModelLayerLocation, LayerDefinition> _roots = new();

    public ModelPart BakeLayer(ModelLayerLocation location)
    {
        if (!_roots.TryGetValue(location, out var layer))
            throw new ArgumentException($"No model for layer {location}");

        return layer.BakeRoot();
    }
    
    public void OnResourceManagerReload(IResourceManager manager)
    {
        _roots = LayerDefinition.CreateRoots();
    }
}