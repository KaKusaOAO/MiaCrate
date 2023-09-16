using System.IO.Compression;
using System.Reflection;

namespace MiaCrate.Resources;

public static class ResourceAssembly
{
    private static readonly Lazy<ZipArchive> _lazyArchive = new(GetGameArchive);
    
    public static Assembly Assembly => typeof(ResourceAssembly).Assembly;
    public static ZipArchive GameArchive => _lazyArchive.Value;

    private static ZipArchive GetGameArchive()
    {
        var stream = GetResource("1.20.2-pre2.jar");
        if (stream == null)
            throw new IOException("Cannot get game archive");

        return new ZipArchive(stream, ZipArchiveMode.Read);
    }

    public static Stream? GetResource(string path)
    {
        path = path.Replace('\\', '/').Replace('/', '.');
        path = typeof(ResourceAssembly).Namespace + "." + path;
        return Assembly.GetManifestResourceStream(path);
    }
}