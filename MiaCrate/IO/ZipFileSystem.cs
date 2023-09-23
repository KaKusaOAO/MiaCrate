using System.IO.Compression;

namespace MiaCrate.IO;

public class ZipFileSystem : IFileSystem
{
    private readonly string _baseDir;

    public string BasePath => _baseDir;
    public ZipArchive Container { get; }

    public ZipFileSystem(ZipArchive archive, string baseDir = "/")
    {
        Container = archive;
        _baseDir = baseDir;
    }

    private string ResolvePath(string path) => 
        Path.GetRelativePath("/", Path.Combine(_baseDir, path))
            .Replace('\\', '/');

    private Stream InternalOpen(string path)
    {
        path = ResolvePath(path);
        var entry = Container.GetEntry(path);
        if (entry == null)
        {
            throw new FileNotFoundException($"Entry {path} not found");
        }
        
        return entry.Open();
    }

    public Stream Open(string path, FileMode mode) => 
        InternalOpen(path);

    public Stream Open(string path, FileMode mode, FileAccess access) => 
        InternalOpen(path);

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share) => 
        InternalOpen(path);

    public string[] GetDirectories(string path)
    {
        path = ResolvePath(path);
        var splitted = PathHelper.Normalize(path)
            .Replace('\\', '/')
            .Split('/');

        return Container.Entries
            .Select(x => x.FullName.Replace('\\', '/'))
            .Select(x => x.Split('/'))
            .Where(x => x.Length > splitted.Length + 1)
            .Where(x => x.Take(splitted.Length).SequenceEqual(splitted))
            .Select(x => x.Take(splitted.Length + 1).Last())
            .ToHashSet()
            .ToArray();
    }
    
    public string[] GetDirectories()
    {
        var path = ResolvePath(_baseDir);
        var splitted = PathHelper.Normalize(path)
            .Replace('\\', '/')
            .Split('/');

        return Container.Entries
            .Select(x => x.FullName.Replace('\\', '/'))
            .Select(x => x.Split('/'))
            .Where(x => x.Length > splitted.Length + 1)
            .Where(x => x.Take(splitted.Length).SequenceEqual(splitted))
            .Select(x => x.Take(splitted.Length + 1).Last())
            .ToHashSet()
            .ToArray();
    }
    
    public string[] GetFiles(string path)
    {
        path = ResolvePath(path);
        var splitted = PathHelper.Normalize(path)
            .Replace('\\', '/')
            .Split('/');

        return Container.Entries
            .Select(x => x.FullName.Replace('\\', '/'))
            .Select(x => x.Split('/'))
            .Where(x => x.Length == splitted.Length + 1)
            .Where(x => x.Take(splitted.Length).SequenceEqual(splitted))
            .Select(x => x.Take(splitted.Length + 1).Last())
            .ToHashSet()
            .ToArray();
    }
    
    public string[] GetFiles()
    {
        var path = ResolvePath(_baseDir);
        var splitted = PathHelper.Normalize(path)
            .Replace('\\', '/')
            .Split('/');

        return Container.Entries
            .Select(x => x.FullName.Replace('\\', '/'))
            .Select(x => x.Split('/'))
            .Where(x => x.Length == splitted.Length + 1)
            .Where(x => x.Take(splitted.Length).SequenceEqual(splitted))
            .Select(x => x.Take(splitted.Length + 1).Last())
            .ToHashSet()
            .ToArray();
    }
}