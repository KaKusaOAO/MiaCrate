using System.IO.Compression;
using System.Reflection;

namespace MiaCrate.Resources;

public static class ResourceAssembly
{
    private const string GameArchiveName = "1.20.2-pre2.jar";
    private const string SodiumArchiveName = "sodium-fabric-mc1.20.2-0.5.3.jar";
    private const string IrisArchiveName = "iris-mc1.20.2-1.6.9.jar";

    private static readonly Dictionary<string, ZipArchive> _archives = new();
    
    public static Assembly Assembly => typeof(ResourceAssembly).Assembly;
    
    public static ZipArchive GameArchive => _archives.ComputeIfAbsent(GameArchiveName, GetArchive);
    public static ZipArchive SodiumArchive => _archives.ComputeIfAbsent(SodiumArchiveName, GetArchive);
    public static ZipArchive IrisArchive => _archives.ComputeIfAbsent(IrisArchiveName, GetArchive);

    private static ZipArchive GetArchive(string name)
    {
        var stream = GetResource(name);
        if (stream == null)
            throw new IOException($"Cannot get archive: {name}");

        return new ZipArchive(stream, ZipArchiveMode.Read);
    }

    public static Stream? GetResource(string path)
    {
        path = path.Replace('\\', '/').Replace('/', '.');
        path = typeof(ResourceAssembly).Namespace + "." + path;
        return Assembly.GetManifestResourceStream(path);
    }

    private static TValue ComputeIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> compute)
    {
        if (dict.TryGetValue(key, out var result)) return result;

        var value = compute(key);
        dict.Add(key, value);
        return value;
    }
}