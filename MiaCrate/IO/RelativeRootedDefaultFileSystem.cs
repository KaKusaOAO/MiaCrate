namespace MiaCrate.IO;

public class RelativeRootedDefaultFileSystem : DefaultFileSystem
{
    private readonly string _basePath;

    public RelativeRootedDefaultFileSystem(string basePath)
    {
        _basePath = basePath;
    }
    
    protected override string ResolvePath(string path)
    {
        if (Path.IsPathFullyQualified(path))
            throw new Exception("Path cannot be fully qualified here");
        
        return Path.GetFullPath(path, _basePath);
    }
}