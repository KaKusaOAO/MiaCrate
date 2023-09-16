using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;

namespace MiaCrate.Client.UI;

public class GuiGraphics
{
    private readonly Game _game;

    public PoseStack Pose { get; }
    public IMultiBufferSource.BufferSource BufferSource { get; }
    public int GuiWidth => _game.Window.GuiScaledWidth;
    public int GuiHeight => _game.Window.GuiScaledHeight;

    private GuiGraphics(Game game, PoseStack poseStack, IMultiBufferSource.BufferSource bufferSource)
    {
        _game = game;
        Pose = poseStack;
        BufferSource = bufferSource;
    }
    
    public GuiGraphics(Game game, IMultiBufferSource.BufferSource bufferSource) 
        : this(game, new PoseStack(), bufferSource) { }

    public void Flush()
    {
        RenderSystem.DisableDepthTest();
        BufferSource.EndBatch();
        RenderSystem.EnableDepthTest();
    }

    private void InnerBlit(ResourceLocation location, int i, int j, int k, int l, int m, float f, float g, float h,
        float n)
    {
        RenderSystem.SetShaderTexture(0, location);
        RenderSystem.SetShader(() => GameRenderer.PositionTexShader);
    }
}