namespace MiaCrate.Resources;

public class PackType
{
    private static readonly Dictionary<int, PackType> _values = new();

    public static List<PackType> Values => _values.Values.ToList(); 
    public string Directory { get; }
    public int Ordinal { get; }

    public static readonly PackType ClientResources = new("assets");
    public static readonly PackType ServerData = new("data");
    
    private PackType(string directory)
    {
        Directory = directory;
        
        Ordinal = _values.Count;
        _values.Add(Ordinal, this);
    }
}