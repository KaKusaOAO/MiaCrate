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

    public void ListResources(PackType type, string str, string path, IPackResources.ResourceOutputDelegate output)
    {
        var pathList = path.Replace('\\', '/').Split('/').ToList();
        var fsList = _pathsForType[type];
        var i = fsList.Count;
        
        if (i == 1)
        {
            GetResources(output, str, fsList.First(), pathList);
        } else if (i > 1)
        {
            var dict = new Dictionary<ResourceLocation, Func<Stream>>();

            for (var j = 0; j < i - 1; j++)
            {
                GetResources((location, stream) => dict.TryAdd(location, stream),
                    str, fsList[j], pathList);
            }

            var fs = fsList[i - 1];
            if (!dict.Any())
            {
                GetResources(output, str, fs, pathList);
            }
            else
            {
                GetResources((location, stream) => dict.TryAdd(location, stream),
                    str, fs, pathList);
                
                foreach (var (key, value) in dict)
                {
                    output(key, value);
                }
            }
        }
    }

    private static void GetResources(IPackResources.ResourceOutputDelegate output, string str, IFileSystem fs,
        List<string> list)
    {
        var fs2 = fs.CreateRelative(str);
        PathPackResources.ListPath(str, fs2, list, output);
    }

    public ISet<string> GetNamespaces(PackType type) => _namespaces;

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