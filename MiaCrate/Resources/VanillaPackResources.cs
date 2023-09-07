namespace MiaCrate.Resources;

public class VanillaPackResources : IPackResources
{
    private readonly BuiltInMetadata _metadata;
    private readonly HashSet<string> _namespaces;
    private readonly List<string> _rootPaths;
    private readonly Dictionary<PackType, List<string>> _pathsForType;
    
    public string PackId => "vanilla";

    public bool IsBuiltin => true;

    public VanillaPackResources(BuiltInMetadata metadata, HashSet<string> namespaces, List<string> rootPaths,
        Dictionary<PackType, List<string>> pathsForType)
    {
        _metadata = metadata;
        _namespaces = namespaces;
        _rootPaths = rootPaths;
        _pathsForType = pathsForType;
    }
    
    public Func<Stream>? GetResource(PackType type, ResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public Func<Stream>? GetRootResource(params string[] str)
    {
        throw new NotImplementedException();
    }

    public void ListResources(PackType type, string str, string str2, IPackResources.ResourceOutputDelegate output)
    {
        throw new NotImplementedException();
    }

    public ISet<string> GetNamespaces(PackType type)
    {
        throw new NotImplementedException();
    }

    public T? GetMetadataSection<T>(IMetadataSectionSerializer<T> serializer)
    {
        throw new NotImplementedException();
    }

    public void ListRawPaths(PackType type, ResourceLocation location, Action<string> populate)
    {
        
    }
    
    public void Dispose() { }
}