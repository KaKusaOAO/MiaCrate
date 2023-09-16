using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;

namespace MiaCrate.Client;

public static class BufferUploader
{
    public static void DrawWithShader(BufferBuilder.RenderedBuffer renderedBuffer) => 
        RenderSystem.EnsureOnRenderThreadOrInit(() => InternalDrawWithShader(renderedBuffer));

    private static void InternalDrawWithShader(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        var buffer = Upload(renderedBuffer);
        // buffer?.DrawWithShader(RenderSystem.Model);
        throw new NotImplementedException();
    }

    private static VertexBuffer? Upload(BufferBuilder.RenderedBuffer renderedBuffer)
    {
        RenderSystem.AssertOnRenderThread();
        if (renderedBuffer.IsEmpty)
        {
            renderedBuffer.Dispose();
            return null;
        }

        throw new NotImplementedException();
    }
}

