using System.Reflection;

namespace MiaCrate.Resources;

public static class ResourceAssembly
{
    public static Assembly Assembly => typeof(ResourceAssembly).Assembly;

    public static Stream? GetResource(string path)
    {
        path = path.Replace('\\', '/').Replace('/', '.');
        path = typeof(ResourceAssembly).Namespace + "." + path;
        return Assembly.GetManifestResourceStream(path);
    }
}