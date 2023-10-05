using MiaCrate.Client.Graphics;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;

namespace MiaCrate.Client;

public static class BufferUploader
{
    private static VertexBuffer? _lastImmediateBuffer;

    public static void Reset()
    {
        if (_lastImmediateBuffer == null) return;
        
        Invalidate();
        VertexBuffer.Unbind();
    }

    public static void Invalidate()
    {
        _lastImmediateBuffer = null;
    }

    public static void Draw(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        var vertexBuffer = Upload(renderedBuffer);
        vertexBuffer?.Draw();
    }
    
    public static void DrawWithShader(BufferBuilder.RenderedBuffer renderedBuffer) => 
        RenderSystem.EnsureOnRenderThreadOrInit(() => InternalDrawWithShader(renderedBuffer));

    private static void InternalDrawWithShader(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        var buffer = Upload(renderedBuffer);
        buffer?.DrawWithShader(RenderSystem.ModelViewMatrix, RenderSystem.ProjectionMatrix, RenderSystem.Shader!);
    }

    private static VertexBuffer? Upload(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        RenderSystem.AssertOnRenderThread();
        if (renderedBuffer.IsEmpty)
        {
            renderedBuffer.Dispose();
            return null;
        }

        var buffer = BindImmediateBuffer(renderedBuffer.DrawState.Format);
        buffer.Upload(renderedBuffer);
        return buffer;
    }

    private static VertexBuffer BindImmediateBuffer(VertexFormat format)
    {
        var buffer = format.ImmediateDrawVertexBuffer;
        BindImmediateBuffer(buffer);
        return buffer;
    }
    
    private static void BindImmediateBuffer(VertexBuffer buffer)
    {
        if (buffer == _lastImmediateBuffer) return;
        buffer.Bind();
        _lastImmediateBuffer = buffer;
    }
}

