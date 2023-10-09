using System.IO.Compression;
using MiaCrate.IO;

namespace MiaCrate.Resources;

public class ModPackResources : IPackResources
{
    private readonly ZipFileSystem _archive;

    public string PackId { get; }

    public ModPackResources(string id, ZipArchive archive)
    {
        PackId = id;
        _archive = new ZipFileSystem(archive);
    }
    
    public void Dispose()
    {
        
    }

    public Func<Stream>? GetResource(PackType type, ResourceLocation location)
    {
        try
        {
            var path = Path.Combine(type.Directory, Path.Combine(location.Namespace, location.Path));
            var stream = _archive.Open(path, FileMode.Open);
            return () => stream;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public Func<Stream>? GetRootResource(params string[] str)
    {
        var path = string.Join('/', str);
        
        try
        {
            var stream = _archive.Open(path, FileMode.Open);
            return () => stream;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void ListResources(PackType type, string str, string path, IPackResources.ResourceOutputDelegate output)
    {
        var pathList = path.Replace('\\', '/').Split('/').ToList();
        GetResources(output, str, _archive, pathList);
    }
    
    private static void GetResources(IPackResources.ResourceOutputDelegate output, string str, IFileSystem fs,
        List<string> list)
    {
        var fs2 = fs.CreateRelative(str);
        PathPackResources.ListPath(str, fs2, list, output);
    }

    public ISet<string> GetNamespaces(PackType type) => _archive.GetDirectories(type.Directory).ToHashSet();

    public T? GetMetadataSection<T>(IMetadataSectionSerializer<T> serializer) where T : class
    {
        var supplier = GetRootResource("pack.mcmeta");
        if (supplier == null) return null;

        using var stream = supplier();
        return AbstractPackResources.GetMetadataFromStream(serializer, stream);
    }
}