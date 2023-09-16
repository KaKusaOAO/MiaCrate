using MiaCrate.IO;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class VanillaPackResources : IPackResources
{
    private readonly BuiltInMetadata _metadata;
    private readonly HashSet<string> _namespaces;
    private readonly List<IFileSystem> _rootPaths;
    private readonly Dictionary<PackType, List<IFileSystem>> _pathsForType;
    
    public string PackId => "vanilla";

    public bool IsBuiltin => true;

    public VanillaPackResources(BuiltInMetadata metadata, HashSet<string> namespaces, List<IFileSystem> rootPaths,
        Dictionary<PackType, List<IFileSystem>> pathsForType)
    {
        _metadata = metadata;
        _namespaces = namespaces;
        _rootPaths = rootPaths;
        _pathsForType = pathsForType;
    }
    
    public Func<Stream>? GetResource(PackType type, ResourceLocation location)
    {
        foreach (var fs in _pathsForType[type])
        {
            try
            {
                var path = Path.Combine(location.Namespace, location.Path);
                var stream = fs.Open(path, FileMode.Open);
                return () => stream;
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
            }
        }

        return null;
    }

    public Func<Stream>? GetRootResource(params string[] str)
    {
        var path = string.Join('/', str);

        foreach (var fs in _rootPaths)
        {
            try
            {
                var stream = fs.Open(path, FileMode.Open);
                return () => stream;
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);
            }
        }

        return null;
    }

    public void ListResources(PackType type, string str, string str2, IPackResources.ResourceOutputDelegate output)
    {
        throw new NotImplementedException();
    }

    public ISet<string> GetNamespaces(PackType type)
    {
        throw new NotImplementedException();
    }

    public T? GetMetadataSection<T>(IMetadataSectionSerializer<T> serializer) where T : class
    {
        var supplier = GetRootResource("pack.mcmeta");
        var builtIn = _metadata.Get(serializer);
        if (supplier == null) return builtIn;

        using var stream = supplier();
        return AbstractPackResources.GetMetadataFromStream(serializer, stream) ?? builtIn;
    }

    public void ListRawPaths(PackType type, ResourceLocation location, Action<string> populate)
    {
        
    }

    public IResourceProvider AsProvider() => new Provider(this);

    private class Provider : IResourceProvider
    {
        private readonly VanillaPackResources _resources;

        public Provider(VanillaPackResources resources)
        {
            _resources = resources;
        }

        public IOptional<Resource> GetResource(ResourceLocation location) =>
            Optional.OfNullable(_resources.GetResource(PackType.ClientResources, location))
                .Select(s => new Resource(_resources, s));
    }
    
    public void Dispose() { }
}