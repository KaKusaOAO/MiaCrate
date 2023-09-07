namespace MiaCrate;

public static class FileHelper
{
    public static string GetFullResourcePath(string path) =>
        (Path.GetDirectoryName(Path.GetFullPath(path, "/")) ?? string.Empty)
            .Replace(Path.PathSeparator, '/');
}