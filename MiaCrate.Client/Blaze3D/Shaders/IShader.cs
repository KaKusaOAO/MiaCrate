namespace MiaCrate.Client.Shaders;

public interface IShader
{
    public int Id { get; }
    public void MarkDirty();
    public Program VertexProgram { get; }
    public Program FragmentProgram { get; }
    public void AttachToProgram();
}