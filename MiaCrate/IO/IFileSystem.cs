namespace MiaCrate.IO;

public interface IFileSystem
{
    public Stream Open(string path, FileMode mode);
    public Stream Open(string path, FileMode mode, FileAccess access);
    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share);
    public string[] GetDirectories(string path);
    public string[] GetFiles(string directory);
}