namespace MiaCrate.IO;

public class DefaultFileSystem : IFileSystem
{
    public virtual string BasePath => "/";

    protected virtual string ResolvePath(string path) => path;
    
    public Stream Open(string path, FileMode mode) =>
        File.Open(ResolvePath(path), mode);

    public Stream Open(string path, FileMode mode, FileAccess access) =>
        File.Open(ResolvePath(path), mode, access);

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share) =>
        File.Open(ResolvePath(path), mode, access, share);

    public string[] GetDirectories(string path) =>
        Directory.GetDirectories(ResolvePath(path));

    public string[] GetFiles(string directory) => 
        Directory.GetFiles(ResolvePath(directory));
    
    public string[] GetDirectories() =>
        Directory.GetDirectories(Path.GetFullPath(BasePath));

    public string[] GetFiles() => 
        Directory.GetFiles(Path.GetFullPath(BasePath));
}