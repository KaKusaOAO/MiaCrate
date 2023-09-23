using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;

namespace MiaCrate.Client.UI;

public class GuiGraphics
{
    private readonly Game _game;
    private bool _managed;
    private readonly ScissorStack _scissorStack;

    public PoseStack Pose { get; }
    public IMultiBufferSource.BufferSource BufferSource { get; }
    public int GuiWidth => _game.Window.GuiScaledWidth;
    public int GuiHeight => _game.Window.GuiScaledHeight;

    private GuiGraphics(Game game, PoseStack poseStack, IMultiBufferSource.BufferSource bufferSource)
    {
        _scissorStack = new ScissorStack();
        _game = game;
        Pose = poseStack;
        BufferSource = bufferSource;
    }
    
    public GuiGraphics(Game game, IMultiBufferSource.BufferSource bufferSource) 
        : this(game, new PoseStack(), bufferSource) { }

    public void DrawManaged(Action action)
    {
        Flush();
        _managed = true;
        action();
        _managed = false;
        Flush();
    }

    public void Flush()
    {
        RenderSystem.DisableDepthTest();
        BufferSource.EndBatch();
        RenderSystem.EnableDepthTest();
    }

    private void FlushIfUnmanaged()
    {
        if (_managed) return;
        Flush();
    }
    
    private void FlushIfManaged()
    {
        if (!_managed) return;
        Flush();
    }

    public void Fill(int x1, int y1, int x2, int y2, int color) => 
        Fill(x1, y1, x2, y2, 0, color);
    
    public void Fill(RenderType renderType, int x1, int y1, int x2, int y2, int color) => 
        Fill(renderType, x1, y1, x2, y2, 0, color);

    public void Fill(int x1, int y1, int x2, int y2, int z, int color) => 
        Fill(RenderType.Gui, x1, y1, x2, y2, z, color);

    public void Fill(RenderType renderType, int x1, int y1, int x2, int y2, int z, int color)
    {
        var matrix = Pose.Last.Pose;

        if (x1 < x2)
        {
            (x1, x2) = (x2, x1);
        }

        if (y1 < y2)
        {
            (y1, y2) = (y2, y1);
        }

        var a = FastColor.ARGB32.Alpha(color) / 255f;
        var r = FastColor.ARGB32.Red(color) / 255f;
        var g = FastColor.ARGB32.Green(color) / 255f;
        var b = FastColor.ARGB32.Blue(color) / 255f;

        var consumer = BufferSource.GetBuffer(renderType);
        consumer.Vertex(matrix, x1,  y1, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x1,  y2, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x2,  y2, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x2,  y1, z).Color(r, g, b, a).EndVertex();
        
        GlStateManager.PushDebugGroup("GuiGraphics::Fill");
        FlushIfUnmanaged();
        GlStateManager.PopDebugGroup();
    }

    public void SetColor(float r, float g, float b, float a)
    {
        FlushIfManaged();
        RenderSystem.SetShaderColor(r, g, b, a);
    }

    public void Blit(ResourceLocation location, int i, int j, int k, int l, float f, float g, int m, int n, int o, int p)
    {
        Blit(location, i, i + k, j, j + l, 0, m, n, f, g, o, p);
    }

    private void Blit(ResourceLocation location, int i, int j, int k, int l, int m, int n, int o, float f, float g,
        int p, int q)
    {
        InnerBlit(location, i, j, k, l, m, 
            (f + 0f) / p, (f + n) / p, 
            (g + 0f) / q, (g + o) / q);
    }
    
    private void InnerBlit(ResourceLocation location, int i, int j, int k, int l, int m, float f, float g, float h,
        float n)
    {
        RenderSystem.SetShaderTexture(0, location);
        RenderSystem.SetShader(() => GameRenderer.PositionTexShader);

        var matrix = Pose.Last.Pose;
        var builder = Tesselator.Instance.Builder;
        builder.Begin(VertexFormat.Mode.Quads, DefaultVertexFormat.PositionTex);
        builder.Vertex(matrix, i,  k, m).Uv(f, h).EndVertex();
        builder.Vertex(matrix, i,  l, m).Uv(f, n).EndVertex();
        builder.Vertex(matrix, j,  l, m).Uv(g, n).EndVertex();
        builder.Vertex(matrix, j,  k, m).Uv(g, h).EndVertex();
        
        GlStateManager.PushDebugGroup("GuiGraphics::InnerBlit");
        BufferUploader.DrawWithShader(builder.End());
        GlStateManager.PopDebugGroup();
    }

    private class ScissorStack
    {
        private readonly Stack<ScreenRectangle> _stack = new();

        public ScreenRectangle Push(ScreenRectangle rect)
        {
            if (_stack.Any())
            {
                var r2 = _stack.Peek();
                var r3 = rect.Intersection(r2) ?? ScreenRectangle.Empty;
                _stack.Push(r3);
                return r3;
            }

            _stack.Push(rect);
            return rect;
        }

        public ScreenRectangle? Pop()
        {
            if (!_stack.Any())
                throw new InvalidOperationException("Scissor stack underflow");

            _stack.Pop();
            return _stack.Any() ? _stack.Peek() : null;
        }
    }
}