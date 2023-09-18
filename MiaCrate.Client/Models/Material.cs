namespace MiaCrate.Client.Models;

public class Material
{
    public ResourceLocation AtlasLocation { get; }
    public ResourceLocation Texture { get; }

    public Material(ResourceLocation atlasLocation, ResourceLocation texture)
    {
        AtlasLocation = atlasLocation;
        Texture = texture;
    }   
}