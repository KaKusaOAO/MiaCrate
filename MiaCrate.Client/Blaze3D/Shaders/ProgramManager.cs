using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using Veldrid;

namespace MiaCrate.Client.Shaders;

public static class ProgramManager
{
    public static void UseProgram(ShaderSetDescription program)
    {
        RenderSystem.AssertOnRenderThread();
        GlStateManager.UseProgram(program);
    }

    public static void ReleaseProgram(IShader shader)
    {
        RenderSystem.AssertOnRenderThread();
        shader.FragmentProgram.Dispose();
        shader.VertexProgram.Dispose();
    }
}