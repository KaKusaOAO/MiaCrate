using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Pipeline;

public abstract class RenderTarget
{
    private const int RedChannel = 0;
    private const int GreenChannel = 0;
    private const int BlueChannel = 0;
    private const int AlphaChannel = 0;

    private readonly float[] _clearChannels = { 1f, 1f, 1f, 0f };
    
    public int Width { get; set; }
    public int Height { get; set; }
    public int ViewWidth { get; set; }
    public int ViewHeight { get; set; }
    public bool UseDepth { get; }
    public int FramebufferId { get; set; }
    public int ColorTextureId { get; protected set; }
    public int DepthBufferId { get; protected set; }
    public TextureMinFilter FilterMode { get; set; }

    protected RenderTarget(bool useDepth)
    {
        UseDepth = useDepth;
        FramebufferId = -1;
        ColorTextureId = -1;
        DepthBufferId = -1;
    }
    
    public void UnbindWrite() => 
        RenderSystem.EnsureOnRenderThread(() => 
            GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0));

    public void Resize(int width, int height, bool clearError) => 
        RenderSystem.EnsureOnRenderThread(() => 
            InternalResize(width, height, clearError));

    private void InternalResize(int width, int height, bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.EnableDepthTest();
        if (FramebufferId >= 0) DestroyBuffers();
        
        CreateBuffers(width, height, clearError);
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void CopyDepthFrom(RenderTarget target)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.BindFramebuffer(FramebufferTarget.ReadFramebuffer, target.FramebufferId);
        GlStateManager.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FramebufferId);
        GlStateManager.BlitFramebuffer(0, 0, target.Width, target.Height, 0, 0, Width, Height,
            ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DestroyBuffers()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        UnbindRead();
        UnbindWrite();

        if (DepthBufferId > -1)
        {
            TextureUtil.ReleaseTextureId(DepthBufferId);
            DepthBufferId = -1;
        }

        if (ColorTextureId > -1)
        {
            TextureUtil.ReleaseTextureId(ColorTextureId);
            ColorTextureId = -1;
        }

        if (FramebufferId > -1)
        {
            GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GlStateManager.DeleteFramebuffers(FramebufferId);
            FramebufferId = -1;
        }
    }

    public void CreateBuffers(int width, int height, bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        var k = RenderSystem.MaxSupportedTextureSize;
        if (width <= 0 || width > k || height <= 0 || height > k)
            throw new ArgumentException($"Window {width}x{height} size out of bounds (max. size: {k})");

        ViewWidth = width;
        ViewHeight = height;
        Width = width;
        Height = height;
        FramebufferId = GlStateManager.GenFramebuffers();
        ColorTextureId = TextureUtil.GenerateTextureId();

        if (UseDepth)
        {
            DepthBufferId = TextureUtil.GenerateTextureId();
            GlStateManager.BindTexture(DepthBufferId);
            GlStateManager.TexMinFilter(TextureTarget.Texture2D, TextureMinFilter.Nearest);
            GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Nearest);
            GlStateManager.TexCompareMode(TextureTarget.Texture2D, TextureCompareMode.None);
            GlStateManager.TexWrapS(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
            GlStateManager.TexWrapT(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
            GlStateManager.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Width, Height, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        }

        SetFilterMode(TextureMinFilter.Nearest);
        GlStateManager.BindTexture(ColorTextureId);
        GlStateManager.TexWrapS(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.TexWrapT(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);
        GlStateManager.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, ColorTextureId, 0);

        if (UseDepth)
        {
            GlStateManager.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, DepthBufferId, 0);
        }

        CheckStatus();
        Clear(clearError);
        UnbindRead();
    }

    public void Clear(bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        BindWrite(true);
        GlStateManager.ClearColor(
            _clearChannels[RedChannel],
            _clearChannels[GreenChannel],
            _clearChannels[BlueChannel],
            _clearChannels[AlphaChannel]);

        var mask = ClearBufferMask.ColorBufferBit;
        if (UseDepth)
        {
            GlStateManager.ClearDepth(1.0);
            mask |= ClearBufferMask.DepthBufferBit;
        }
        
        GlStateManager.Clear(mask, clearError);
        UnbindWrite();
    }

    public void BindWrite(bool resetViewport) => 
        RenderSystem.EnsureOnRenderThread(() => 
            InternalBindWrite(resetViewport));

    private void InternalBindWrite(bool resetViewport)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);
        if (resetViewport)
        {
            GlStateManager.Viewport(0, 0, ViewWidth, ViewHeight);
        }
    }

    public void CheckStatus()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        var status = GlStateManager.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            throw new Exception($"{status}");
    }

    public void SetFilterMode(TextureMinFilter filter)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        FilterMode = filter;
        GlStateManager.BindTexture(ColorTextureId);
        GlStateManager.TexMinFilter(TextureTarget.Texture2D, filter);
        GlStateManager.TexMagFilter(TextureTarget.Texture2D, (TextureMagFilter) filter);
        GlStateManager.BindTexture(0);
    }

    public void UnbindRead()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.BindTexture(0);
    }

    public void SetClearColor(float red, float green, float blue, float alpha)
    {
        _clearChannels[RedChannel] = red;
        _clearChannels[GreenChannel] = green;
        _clearChannels[BlueChannel] = blue;
        _clearChannels[AlphaChannel] = alpha;
    }
}