namespace MiaCrate.Client.Models;

public class Material
{
    public static IComparer<Material> Comparer { get; } = new ComparerInstance();

    public ResourceLocation AtlasLocation { get; }
    public ResourceLocation Texture { get; }

    public Material(ResourceLocation atlasLocation, ResourceLocation texture)
    {
        AtlasLocation = atlasLocation;
        Texture = texture;
    }

    private class ComparerInstance : IComparer<Material>
    {
        public int Compare(Material? x, Material? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            
            var atlasLocationComparison = x.AtlasLocation.CompareTo(y.AtlasLocation);
            if (atlasLocationComparison != 0) return atlasLocationComparison;
            return x.Texture.CompareTo(y.Texture);
        }
    }
}