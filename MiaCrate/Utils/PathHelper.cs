namespace MiaCrate;

public static class PathHelper
{
    public static string Normalize(string path)
    {
        var basePath = Path.GetFullPath("/");
        return Path.GetRelativePath(basePath, Path.GetFullPath(path, basePath))
            .Replace('\\', '/');
    }
}