namespace Mochi;

public class ResourceLocation
{
    public const string DefaultNamespace = "minecraft";
    public string Namespace { get; set; }
    public string Path { get; set; }
    
    public ResourceLocation(string path) : this(DefaultNamespace, path) { }

    public ResourceLocation(string @namespace, string path)
    {
        Namespace = @namespace;
        Path = path;
        
        // TODO: Validate namespace and path
    }

    public static implicit operator string(ResourceLocation location) => location.ToString();

    public static implicit operator ResourceLocation(string raw)
    {
        if (!raw.Contains(':')) return new ResourceLocation(raw);
        
        var split = raw.Split(':', 2);
        return new ResourceLocation(split[0], split[1]);
    }

    public static bool operator ==(ResourceLocation a, ResourceLocation b) => a.ToString() == b.ToString();
    public static bool operator !=(ResourceLocation a, ResourceLocation b) => !(a == b);

    public override string ToString()
    {
        return $"{Namespace}:{Path}";
    }
}