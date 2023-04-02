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
        
        // Validate namespace and path
        if (Namespace.Contains(':')) throw new ArgumentException("Namespace cannot contain ':'");
        if (Path.Contains(':')) throw new ArgumentException("Path cannot contain ':'");
        
        // Validate namespace
        if (Namespace.Length == 0) throw new ArgumentException("Namespace cannot be empty");
        if (Namespace.Length > 32) throw new ArgumentException("Namespace cannot be longer than 32 characters");
        if (Namespace.Any(c => !char.IsLetterOrDigit(c) && c != '_')) throw new ArgumentException("Namespace can only contain letters, digits and underscores");
        
        // Validate path
        if (Path.Length == 0) throw new ArgumentException("Path cannot be empty");
        if (Path.Length > 256) throw new ArgumentException("Path cannot be longer than 256 characters");
        if (Path.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '/' && c != '.')) throw new ArgumentException("Path can only contain letters, digits, underscores, slashes and dots");
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