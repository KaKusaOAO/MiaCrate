using Veldrid;

namespace MiaCrate.Client.Shaders;

public sealed class ProgramType
{
    private static readonly Dictionary<int, ProgramType> _values = new();

    public static readonly ProgramType Vertex = new("vertex", ".vsh", ShaderStages.Vertex);
    public static readonly ProgramType Fragment = new("fragment", ".fsh", ShaderStages.Fragment);
    
    public string Name { get; }
    public string Extension { get; }
    public ShaderStages Type { get; }
    public int Ordinal { get; }
    public Dictionary<string, Program.ConvertResult> Programs { get; } = new();

    private ProgramType(string name, string extension, ShaderStages type)
    {
        Name = name;
        Extension = extension;
        Type = type;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values.Add(ordinal, this);
    }

    public static List<ProgramType> GetValues() => _values.Values.ToList();
}