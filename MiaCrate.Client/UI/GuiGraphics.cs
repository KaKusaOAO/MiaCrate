using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using SkiaSharp;

namespace MiaCrate.Client.UI;

public class GuiGraphics
{
    private readonly Game _game;
    private bool _managed;
    private readonly ScissorStack _scissorStack;
    private readonly GuiSpriteManager _sprites;

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
        _sprites = _game.GuiSprites;
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

    public void Fill(int x1, int y1, int x2, int y2, Argb32 color) => 
        Fill(x1, y1, x2, y2, 0, color);
    
    public void Fill(RenderType renderType, int x1, int y1, int x2, int y2, Argb32 color) => 
        Fill(renderType, x1, y1, x2, y2, 0, color);

    public void Fill(int x1, int y1, int x2, int y2, int z, Argb32 color) => 
        Fill(RenderType.Gui, x1, y1, x2, y2, z, color);

    public void Fill(RenderType renderType, int x1, int y1, int x2, int y2, int z, Argb32 color)
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

        var a = color.Alpha / 255f;
        var r = color.Red / 255f;
        var g = color.Green / 255f;
        var b = color.Blue / 255f;

        var consumer = BufferSource.GetBuffer(renderType);
        consumer.Vertex(matrix, x1,  y1, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x1,  y2, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x2,  y2, z).Color(r, g, b, a).EndVertex();
        consumer.Vertex(matrix, x2,  y1, z).Color(r, g, b, a).EndVertex();
        
        GlStateManager.PushDebugGroup("GuiGraphics::Fill");
        FlushIfUnmanaged();
        GlStateManager.PopDebugGroup();
    }

    public void FillGradient(int x0, int y0, int x1, int y1, Argb32 fromColor, Argb32 toColor)
    {
        FillGradient(x0, y0, x1, y1, 0, fromColor, toColor);
    }
    
    public void FillGradient(int x0, int y0, int x1, int y1, int z, Argb32 fromColor, Argb32 toColor)
    {
        FillGradient(RenderType.Gui, x0, y0, x1, y1, z, fromColor, toColor);
    }

    public void FillGradient(RenderType renderType, int x0, int y0, int x1, int y1, int z, Argb32 fromColor, Argb32 toColor)
    {
        var consumer = BufferSource.GetBuffer(renderType);
        FillGradient(consumer, x0, y0, x1, y1, z, fromColor, toColor);
        FlushIfUnmanaged();
    }

    /// <summary>
    /// Fill with the gradient created from two specified colors, from top to bottom.
    /// </summary>
    /// <param name="consumer">A vertex buffer builder.</param>
    /// <param name="x0">The X component of the top left point.</param>
    /// <param name="y0">The Y component of the top left point.</param>
    /// <param name="x1">The X component of the bottom right point.</param>
    /// <param name="y1">The Y component of the bottom right point.</param>
    /// <param name="z">The depth of this filled plane.</param>
    /// <param name="fromColor">The top color.</param>
    /// <param name="toColor">The bottom color.</param>
    private void FillGradient(IVertexConsumer consumer, int x0, int y0, int x1, int y1, int z, Argb32 fromColor, Argb32 toColor)
    {
        var a0 = fromColor.Alpha / 255.0F;
        var r0 = fromColor.Red / 255.0F;
        var g0 = fromColor.Green / 255.0F;
        var b0 = fromColor.Blue / 255.0F;
        
        var a1 = toColor.Alpha / 255.0F;
        var r1 = toColor.Red / 255.0F;
        var g1 = toColor.Green / 255.0F;
        var b1 = toColor.Blue / 255.0F;
        
        var matrix = Pose.Last.Pose;
        consumer.Vertex(matrix, x0, y0, z).Color(r0, g0, b0, a0).EndVertex();
        consumer.Vertex(matrix, x0, y1, z).Color(r1, g1, b1, a1).EndVertex();
        consumer.Vertex(matrix, x1, y1, z).Color(r1, g1, b1, a1).EndVertex();
        consumer.Vertex(matrix, x1, y0, z).Color(r0, g0, b0, a0).EndVertex();
    }

    public void SetColor(float r, float g, float b, float a)
    {
        FlushIfManaged();
        RenderSystem.SetShaderColor(r, g, b, a);
    }
    
    public void Blit(ResourceLocation location, int x, int y, int z, float u, float v, int width, int height, int texWidth, int texHeight)
    {
        Blit(location, x, x + width, y, y + height, z, width, height, u, v, texWidth, texHeight);
    }
    
    public void Blit(ResourceLocation location, int x, int y, float u, float v, int width, int height, int texWidth, int texHeight)
    {
        Blit(location, x, y, width, height, u, v, width, height, texWidth, texHeight);
    }

    public void Blit(ResourceLocation location, int x, int y, int width, int height, float u0, float v0, 
        int uWidth, int vHeight, int texWidth, int texHeight)
    {
        Blit(location, x, x + width, y, y + height, 0, uWidth, vHeight, u0, v0, texWidth, texHeight);
    }

    private void Blit(ResourceLocation location, int x1, int x2, int y1, int y2, int z, int uWidth, int vHeight, float u0, float v0,
        int texWidth, int texHeight)
    {
        InnerBlit(location, x1, x2, y1, y2, z, 
            (u0 + 0f) / texWidth, (u0 + uWidth) / texWidth, 
            (v0 + 0f) / texHeight, (v0 + vHeight) / texHeight);
    }
    
    private void InnerBlit(ResourceLocation location, int x1, int x2, int y1, int y2, int z, float u0, float u1, float v0,
        float v1)
    {
        RenderSystem.SetShaderTexture(0, location);
        RenderSystem.SetShader(() => GameRenderer.PositionTexShader);

        var matrix = Pose.Last.Pose;
        var builder = Tesselator.Instance.Builder;
        builder.Begin(VertexFormat.Mode.Quads, DefaultVertexFormat.PositionTex);
        builder.Vertex(matrix, x1,  y1, z).Uv(u0, v0).EndVertex();
        builder.Vertex(matrix, x1,  y2, z).Uv(u0, v1).EndVertex();
        builder.Vertex(matrix, x2,  y2, z).Uv(u1, v1).EndVertex();
        builder.Vertex(matrix, x2,  y1, z).Uv(u1, v0).EndVertex();
        
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

    public void BlitSprite(ResourceLocation location, int x, int y, int width, int height)
    {
        BlitSprite(location, x, y, 0, width, height);
    }
    
    public void BlitSprite(ResourceLocation location, int x, int y, int z, int width, int height)
    {
        var sprite = _sprites.GetSprite(location);
        var scaling = _sprites.GetSpriteScaling(sprite);

        if (scaling is IGuiSpriteScaling.TypeStretch)
        {
            BlitSprite(sprite, x, y, z, width, height);
        } 
        else if (scaling is IGuiSpriteScaling.TypeTile tile)
        {
            BlitTiledSprite(sprite, x, y, z, width, height, 0, 0, tile.Width, tile.Height, tile.Width, tile.Height);
        }
        else if (scaling is IGuiSpriteScaling.TypeNineSlice nineSlice)
        {
            BlitNineSlicedSprite(sprite, nineSlice, x, y, z, width, height);
        }
    }

    public void BlitSprite(ResourceLocation location, int i, int j, int k, int l, int m, int n, int o, int p)
    {
        BlitSprite(location, i, j, k, l, m, n, 0, o, p);
    }
    
    public void BlitSprite(ResourceLocation location, int i, int j, int k, int l, int x, int y, int z, int width, int height)
    {
        var sprite = _sprites.GetSprite(location);
        var scaling = _sprites.GetSpriteScaling(sprite);

        if (scaling is IGuiSpriteScaling.TypeStretch)
        {
            BlitSprite(sprite, i, j, k, l, x, y, z, width, height);
        }
        else
        {
            BlitSprite(sprite, x, y, z, width, height);
        }
    }

    private void BlitSprite(TextureAtlasSprite sprite, int uWidth, int vHeight, int u, int v, int x, int y, int z, int width, int height)
    {
        if (width == 0 || height == 0) return;
        InnerBlit(sprite.AtlasLocation, x, x + width, y, y + height, z,
            sprite.GetU((float) u / uWidth), sprite.GetU((float) (u + width) / uWidth),
            sprite.GetV((float) v / vHeight), sprite.GetV((float) (v + height) / vHeight));
    }
    
    private void BlitSprite(TextureAtlasSprite sprite, int x, int y, int z, int width, int height)
    {
        if (width == 0 || height == 0) return;
        InnerBlit(sprite.AtlasLocation, x, x + width, y, y + height, z, 
            sprite.U0, sprite.U1, sprite.V0, sprite.V1);
    }

    private void BlitTiledSprite(TextureAtlasSprite sprite, int x, int y, int z, int tileWidth, int tileHeight, int u, int v, int texWidth,
        int texHeight, int uWidth, int vHeight)
    {
        if (tileWidth == 0 || tileHeight == 0) return;
        if (texWidth <= 0 || texHeight <= 0)
            throw new ArgumentException($"Tiled sprite texture size must be positive,  got {texWidth}x{texHeight}");

        GlStateManager.WrapWithDebugGroup("GuiGraphics::BlitTiledSprite", () =>
        {
            for (var xOffset = 0; xOffset < tileWidth; xOffset += texWidth)
            {
                var cappedWidth = Math.Min(texWidth, tileWidth - xOffset);

                for (var yOffset = 0; yOffset < tileHeight; yOffset += texHeight)
                {
                    var cappedHeight = Math.Min(texHeight, tileHeight - yOffset);
                    BlitSprite(sprite, uWidth, vHeight, u, v, x + xOffset, y + yOffset, z, cappedWidth, cappedHeight);
                }
            }
        });
    }

    private void BlitNineSlicedSprite(TextureAtlasSprite sprite, IGuiSpriteScaling.TypeNineSlice nineSlice,
        int x, int y, int z, int width, int height)
    {
        var border = nineSlice.Border;
        var n = Math.Min(border.Left, width / 2);
        var o = Math.Min(border.Right, width / 2);
        var p = Math.Min(border.Top, height / 2);
        var q = Math.Min(border.Bottom, height / 2);

        GlStateManager.WrapWithDebugGroup("GuiGraphics::BlitNineSlicedSprite", () =>
        {
            if (width == nineSlice.Width && height == nineSlice.Height)
            {
                // Blit the sprite directly as we don't have to do additional calculations
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, 0, x, y, z, width, height);
            }
            else if (height == nineSlice.Height)
            {
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, 0, x, y, z, n, height);
                BlitTiledSprite(sprite, x + n, y, z, width - o - n, height ,n, 0, nineSlice.Width - o - n, nineSlice.Height, nineSlice.Width, nineSlice.Height);
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, nineSlice.Width - o, 0, x + width - o, y, z, o, height);
            }
            else if (width == nineSlice.Width)
            {
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, 0, x, y, z, width, p);
                BlitTiledSprite(sprite, x, y + p, z, width, height - q - p, 0, p, nineSlice.Width, nineSlice.Height - q - p, nineSlice.Width, nineSlice.Height);
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, nineSlice.Height - q, x, y + height - q, z, width, q);
            }
            else
            {
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, 0, x, y, z, n, p);
                BlitTiledSprite(sprite, x + n, y, z, width - o - n, p, n, 0, nineSlice.Width - o - n, p, nineSlice.Width, nineSlice.Height);
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, nineSlice.Width - o, 0, x + width - o, y, z, o, p);
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, 0, nineSlice.Height - q, x, y + height - q, z, n, q);
                BlitTiledSprite(sprite, x + n, y + height - q, z, width - o - n, q, n, nineSlice.Height - q, nineSlice.Width - o - n, q, nineSlice.Width, nineSlice.Height);
                BlitSprite(sprite, nineSlice.Width, nineSlice.Height, nineSlice.Width - o, nineSlice.Height - q, x + width - o, y + height - q, z, o, q);
                BlitTiledSprite(sprite, x, y + p, z, n, height - q - p, 0, p, n, nineSlice.Height - q - p, nineSlice.Width, nineSlice.Height);
                BlitTiledSprite(sprite, x + n, y + p, z, width - o - n, height - q - p, n, p, nineSlice.Width - o - n, nineSlice.Height - q - p, nineSlice.Width, nineSlice.Height);
                BlitTiledSprite(sprite, x + width - o, y + p, z, n, height - q - p, nineSlice.Width - o, p, o, nineSlice.Height - q - p, nineSlice.Width, nineSlice.Height);
            }
        });
    }
}