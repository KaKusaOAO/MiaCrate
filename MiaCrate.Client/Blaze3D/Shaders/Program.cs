namespace MiaCrate.Client.Shaders;

public class Program
{
    private const int MaxLogLength = 32678;
    private readonly ProgramType _type;
    public string Name { get; }
    public int Id { get; }

    protected Program(ProgramType type, int id, string name)
    {
        Name = name;
        Id = id;
        _type = type;
    }

    public void AttachToShader(IShader shader)
    {
        
    }
}