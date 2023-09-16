using MiaCrate.Client.Graphics;

namespace MiaCrate.Client.Models;

public class LayerDefinition
{
    private readonly MeshDefinition _mesh;
    private readonly MaterialDefinition _material;

    public static Dictionary<ModelLayerLocation, LayerDefinition> CreateRoots()
    {
        // throw new NotImplementedException();
        return new Dictionary<ModelLayerLocation, LayerDefinition>();
    }

    private LayerDefinition(MeshDefinition mesh, MaterialDefinition material)
    {
        _mesh = mesh;
        _material = material;
    }

    public ModelPart BakeRoot()
    {
        throw new NotImplementedException();
    }

    public static LayerDefinition Create(MeshDefinition mesh, int width, int height) => 
        new(mesh, new MaterialDefinition(width, height));
}