using MiaCrate.Client.Systems;

namespace MiaCrate.Client.Pipeline;

public class TextureRenderTarget : RenderTarget
{
    public TextureRenderTarget(int width, int height, bool useDepth, bool clearError) : base(useDepth)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        Resize(width, height, clearError);
    }
}