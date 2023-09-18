namespace MiaCrate.Resources;

public class FileToIdConverter
{
    private readonly string _prefix;
    private readonly string _extension;

    public FileToIdConverter(string prefix, string extension)
    {
        _prefix = prefix;
        _extension = extension;
    }

    public static FileToIdConverter Json(string prefix) => new(prefix, ".json");

    public ResourceLocation IdToFile(ResourceLocation location) =>
        location.WithPath($"{_prefix}/{location.Path}{_extension}");
    
    public ResourceLocation FileToId(ResourceLocation location)
    {
        var path = location.Path;
        var start = _prefix.Length + 1;
        var end = path.Length - _extension.Length;
        return location.WithPath(path[start..end]);
    }

    public Dictionary<ResourceLocation, Resource> ListMatchingResources(IResourceManager manager) =>
        manager.ListResources(_prefix, l => l.Path.EndsWith(_extension));

    public Dictionary<ResourceLocation, List<Resource>> ListMatchingResourceStacks(IResourceManager manager) =>
        manager.ListResourceStacks(_prefix, l => l.Path.EndsWith(_extension));
}