namespace MiaCrate.IO;

public class GenericRelativeFileSystem : IFileSystem
{
    private readonly IFileSystem _inner;
    private readonly string _basePath;

    public string BasePath { get; }
    
    public GenericRelativeFileSystem(IFileSystem inner, string basePath)
    {
        _inner = inner;
        _basePath = basePath;
        
        BasePath = Path.Combine(_inner.BasePath, _basePath).Replace('\\', '/');
    }

    public Stream Open(string path, FileMode mode)
    {
        path = Path.Combine(_basePath, path).Replace('\\', '/');
        return _inner.Open(path, mode);
    }

    public Stream Open(string path, FileMode mode, FileAccess access)
    {
        path = Path.Combine(_basePath, path).Replace('\\', '/');
        return _inner.Open(path, mode, access);
    }

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        path = Path.Combine(_basePath, path).Replace('\\', '/');
        return _inner.Open(path, mode, access, share);
    }

    public string[] GetDirectories(string path)
    {
        path = Path.Combine(_basePath, path).Replace('\\', '/');
        return _inner.GetDirectories(path);
    }

    public string[] GetDirectories() => _inner.GetDirectories(_basePath);

    public string[] GetFiles(string directory)
    {
        directory = Path.Combine(_basePath, directory).Replace('\\', '/');
        return _inner.GetFiles(directory);
    }

    public string[] GetFiles() => _inner.GetFiles(_basePath);
}