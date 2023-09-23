namespace MiaCrate.IO;

public interface IFileSystem
{
    public string BasePath { get; }
    public Stream Open(string path, FileMode mode);
    public Stream Open(string path, FileMode mode, FileAccess access);
    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share);
    public string[] GetDirectories(string path);
    public string[] GetDirectories();
    public string[] GetFiles(string directory);
    public string[] GetFiles();
}

public static class FileSystemExtension
{
    public static IFileSystem CreateRelative(this IFileSystem fs, string path)
    {
        if (fs is RelativeRootedDefaultFileSystem rrdfs) return rrdfs.CreateRelative(path);
        
        if (fs is ZipFileSystem zip)
            return new ZipFileSystem(zip.Container,
                Path.Combine(zip.BasePath, path).Replace('\\', '/'));
        
        if (fs is DefaultFileSystem dfs) return new RelativeRootedDefaultFileSystem(path);
        
        return new GenericRelativeFileSystem(fs, path);
    }
}