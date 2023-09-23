using MiaCrate.IO;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class PathPackResources : AbstractPackResources
{
    public PathPackResources(string name, bool isBuiltIn) : base(name, isBuiltIn)
    {
    }

    public static void ListPath(string ns, IFileSystem fs, List<string> list,
        IPackResources.ResourceOutputDelegate resourceOutput)
    {
        var basePath = fs.BasePath;
        foreach (var subPath in list)
        {
            fs = fs.CreateRelative(subPath);
        }

        try
        {
            var files = fs.GetFiles().ToList();
            var directories = new List<string>();
            ListDirectories(fs, ".", s => directories.Add(s));
            files.AddRange(directories.SelectMany(d => fs.GetFiles(d).Select(f => d + "/" + f)));

            foreach (var file in files)
            {
                var fullFsPath = Path.Combine(fs.BasePath, file).Replace('\\', '/');
                var path = Path.GetRelativePath(basePath, fullFsPath).Replace('\\', '/');
                if (!ResourceLocation.TryBuild(ns, path, out var location))
                {
                    Util.LogAndPauseIfInIde($"Invalid path in pack: {ns}:{fullFsPath}, ignoring");
                }
                else
                {
                    resourceOutput(location, () => fs.Open(fullFsPath, FileMode.Open));
                }
            }
        }
        catch (DirectoryNotFoundException)
        {

        }
        catch (FileNotFoundException)
        {
            
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to list path {fs}");
            Logger.Error(ex);
        }
    }

    private static void ListDirectories(IFileSystem fs, string path, Action<string> output)
    {
        var directories = path == "."
            ? fs.GetDirectories()
            : fs.GetDirectories(path);
        
        foreach (var directory in directories)
        {
            output(path == "." ? directory : path + "/" + directory);
            ListDirectories(fs, directory, output);
        }
    }

    public override void Dispose()
    {
        // throw new NotImplementedException();
    }
}