using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Shaders;

public static class ProgramManager
{
    public static void UseProgram(int program)
    {
        RenderSystem.AssertOnRenderThread();
        GlStateManager.UseProgram(program);
    }

    public static void ReleaseProgram(IShader shader)
    {
        RenderSystem.AssertOnRenderThread();
        shader.FragmentProgram.Dispose();
        shader.VertexProgram.Dispose();
        GlStateManager.DeleteProgram(shader.Id);
    }

    public static int CreateProgram()
    {
        RenderSystem.AssertOnRenderThread();
        var program = GlStateManager.CreateProgram();
        if (program <= 0)
            throw new IOException($"Could not create shader program (returned program ID {program})");

        return program;
    }

    public static void LinkShader(IShader shader)
    {
        RenderSystem.AssertOnRenderThread();
        shader.AttachToProgram();
        GlStateManager.LinkProgram(shader.Id);

        var status = GlStateManager.GetProgramI(shader.Id, GetProgramParameterName.LinkStatus);
        if (status == 0)
        {
            Logger.Warn($"Error encountered when linking program containing" +
                        $" VS {shader.VertexProgram.Name} and FS {shader.FragmentProgram.Name}. Log output:");
            Logger.Warn(GlStateManager.GetProgramInfoLog(shader.Id));
        }
    }
}