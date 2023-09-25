using System.IO.Compression;
using MiaCrate.Extensions;

namespace MiaCrate.IO;

public class ZipFileSystem : IFileSystem
{
    private static readonly Dictionary<ZipArchive, SemaphoreSlim> _lock = new(); 
    
    private readonly string _baseDir;

    public string BasePath => _baseDir;
    public ZipArchive Container { get; }

    public ZipFileSystem(ZipArchive archive, string baseDir = "/")
    {
        Container = archive;
        _baseDir = baseDir;
    }

    private void Wait()
    {
        var l = _lock.ComputeIfAbsent(Container, _ => new(1, 1));
        l.Wait();
    }
    
    private void Release()
    {
        if (!_lock.TryGetValue(Container, out var l)) return;
        l.Release();
    }

    private string ResolvePath(string path) => 
        Path.GetRelativePath("/", Path.Combine(_baseDir, path))
            .Replace('\\', '/');

    private Stream InternalOpen(string path)
    {
        Wait();
        try
        {
            path = ResolvePath(path);
            var entry = Container.GetEntry(path);
            if (entry == null)
            {
                throw new FileNotFoundException($"Entry {path} not found");
            }

            // For some reason we need to copy the content into memory first
            using var stream = entry.Open();
            var memory = new MemoryStream();
            stream.CopyTo(memory);

            // Reset the position and pass it to caller
            memory.Seek(0, SeekOrigin.Begin);
            return memory;
        }
        finally
        {
            Release();
        }
    }

    public Stream Open(string path, FileMode mode) => 
        InternalOpen(path);

    public Stream Open(string path, FileMode mode, FileAccess access) => 
        InternalOpen(path);

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share) => 
        InternalOpen(path);

    public string[] GetDirectories(string path)
    {
        Wait();
        try
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
        finally
        {
            Release();
        }
    }
    
    public string[] GetDirectories()
    {
        Wait();
        try
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
        finally
        {
            Release();
        }
    }
    
    public string[] GetFiles(string path)
    {
        Wait();
        try
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
        finally
        {
            Release();
        }
    }
    
    public string[] GetFiles()
    {
        Wait();
        try
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
        finally
        {
            Release();
        }
    }
}