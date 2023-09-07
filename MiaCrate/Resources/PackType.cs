namespace MiaCrate.Resources;

public class PackType
{
    public string Directory { get; }

    public static readonly PackType ClientResources = new("assets");
    public static readonly PackType ServerData = new("data");
    
    private PackType(string directory)
    {
        Directory = directory;
    }
}