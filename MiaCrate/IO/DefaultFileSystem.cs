namespace MiaCrate.IO;

public class DefaultFileSystem : IFileSystem
{
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
}