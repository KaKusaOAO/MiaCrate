using MiaCrate.Client.Shaders;

namespace MiaCrate.Client.Graphics;

public class ShaderInstance : IShader
{
    public int Id => throw new NotImplementedException();

    public void MarkDirty()
    {
        throw new NotImplementedException();
    }

    public Program VertexProgram => throw new NotImplementedException();

    public Program FragmentProgram => throw new NotImplementedException();

    public void AttachToProgram()
    {
        throw new NotImplementedException();
    }
}