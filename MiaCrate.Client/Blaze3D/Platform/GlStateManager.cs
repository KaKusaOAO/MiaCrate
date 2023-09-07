using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public static class GlStateManager
{
    private static readonly BlendState _blend = new();
    private static readonly DepthState _depth = new();
    private static readonly CullState _cull = new();
    private static readonly PolygonOffsetState _polyOffset = new();
    private static readonly ColorLogicState _colorLogic = new();
    private static readonly ScissorState _scissor = new();

    public static void AttachShader(int i, int j)
    {
        
    }

    public static string GetString(StringName name)
    {
        RenderSystem.AssertOnRenderThread();
        return GL.GetString(name);
    }

    public static void EnableVertexAttribArray(int index)
    {
        RenderSystem.AssertOnRenderThread();
        GL.EnableVertexAttribArray(index);
    }
    
    public static void DisableVertexAttribArray(int index)
    {
        RenderSystem.AssertOnRenderThread();
        GL.DisableVertexAttribArray(index);
    }
    
    public static void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, IntPtr ptr)
    {
        RenderSystem.AssertOnRenderThread();
        GL.VertexAttribPointer(index, size, type, normalized, stride, ptr);
    }
    
    public static void VertexAttribIPointer(int index, int size, VertexAttribIntegerType type, int stride, IntPtr ptr)
    {
        RenderSystem.AssertOnRenderThread();
        GL.VertexAttribIPointer(index, size, type, stride, ptr);
    }
    
    public static void VertexAttribIPointer(int index, int size, VertexAttribPointerType type, int stride, IntPtr ptr)
    {
        var name = Enum.GetName(type);
        if (Enum.TryParse<VertexAttribIntegerType>(name, out var t))
        {
            VertexAttribIPointer(index, size, t, stride, ptr);
            return;
        }
        
        Logger.Warn($"Unknown {nameof(VertexAttribIntegerType)}: {type}");
    }
}