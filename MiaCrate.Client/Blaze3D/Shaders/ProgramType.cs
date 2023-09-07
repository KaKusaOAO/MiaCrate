using OpenTK.Graphics.OpenGL;

namespace MiaCrate.Client.Shaders;

public sealed class ProgramType
{
    private static readonly Dictionary<int, ProgramType> _values = new();

    public static readonly ProgramType Vertex = new("vertex", ".vsh", ShaderType.VertexShader);
    public static readonly ProgramType Fragment = new("fragment", ".fsh", ShaderType.FragmentShader);
    
    public string Name { get; }
    public string Extension { get; }
    public ShaderType GlType { get; }
    public int Ordinal { get; }
    public Dictionary<string, Program> Programs { get; } = new();

    private ProgramType(string name, string extension, ShaderType glType)
    {
        Name = name;
        Extension = extension;
        GlType = glType;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values.Add(ordinal, this);
    }

    public static List<ProgramType> GetValues() => _values.Values.ToList();
}