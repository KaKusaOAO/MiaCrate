using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using OpenTK.Mathematics;
using Veldrid;

namespace MiaCrate.Client.Pipeline;

public abstract class RenderTarget
{
    private const int RedChannel = 0;
    private const int GreenChannel = 0;
    private const int BlueChannel = 0;
    private const int AlphaChannel = 0;

    private readonly float[] _clearChannels = {1f, 1f, 1f, 0f};

    public int Width { get; set; }
    public int Height { get; set; }
    public int ViewWidth { get; set; }
    public int ViewHeight { get; set; }
    public bool UseDepth { get; }
    public Framebuffer? FramebufferId { get; set; }
    public Texture? ColorTextureId { get; protected set; }
    public Texture? DepthBufferId { get; protected set; }
    public Sampler? ColorTextureSampler { get; protected set; }
    public Sampler? DepthBufferSampler { get; protected set; }
    public SamplerFilter FilterMode { get; set; }

    protected SamplerDescription colorSamplerDescription;
    protected SamplerDescription depthBufferDescription;

    protected RenderTarget(bool useDepth)
    {
        UseDepth = useDepth;
    }

    public void UnbindWrite()
    {
        RenderSystem.EnsureOnRenderThread(() =>
        {
            var cl = GlStateManager.CommandList;
            var device = GlStateManager.Device;
            cl.SetFramebuffer(device.SwapchainFramebuffer);
        });
    }

    public void Resize(int width, int height, bool clearError) =>
        RenderSystem.EnsureOnRenderThread(() =>
            InternalResize(width, height, clearError));

    private void InternalResize(int width, int height, bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.EnableDepthTest();
        if (FramebufferId != null) DestroyBuffers();

        CreateBuffers(width, height, clearError);
    }

    public void CopyDepthFrom(RenderTarget target)
    {
        RenderSystem.AssertOnRenderThreadOrInit();

        var cl = GlStateManager.ResourceFactory.CreateCommandList();
        cl.Begin();
        cl.CopyTexture(target.DepthBufferId, DepthBufferId);
        cl.End();
        GlStateManager.Device.SubmitCommands(cl);

        // GlStateManager.BindFramebuffer(FramebufferTarget.ReadFramebuffer, target.FramebufferId);
        // GlStateManager.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FramebufferId);
        // GlStateManager.BlitFramebuffer(0, 0, target.Width, target.Height, 0, 0, Width, Height,
        //     ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        // GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DestroyBuffers()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        UnbindRead();
        UnbindWrite();

        DepthBufferId?.Dispose();
        DepthBufferSampler?.Dispose();
        ColorTextureId?.Dispose();
        ColorTextureSampler?.Dispose();
        FramebufferId?.Dispose();
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

        var colorTextureDesc = new TextureDescription((uint) Width, (uint) Height, 0, 0, 0,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget, TextureType.Texture2D);
        ColorTextureId = GlStateManager.ResourceFactory.CreateTexture(colorTextureDesc);
        ColorTextureId.Name = "Render Target - Color";

        if (UseDepth)
        {
            var depthTextureDesc = new TextureDescription((uint) Width, (uint) Height, 0, 0, 0,
                PixelFormat.R16_Float, TextureUsage.DepthStencil, TextureType.Texture2D);
            DepthBufferId = GlStateManager.ResourceFactory.CreateTexture(depthTextureDesc);
            DepthBufferId.Name = "Render Target - Depth";

            depthBufferDescription.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
            depthBufferDescription.AddressModeU = SamplerAddressMode.Clamp;
            depthBufferDescription.AddressModeV = SamplerAddressMode.Clamp;
            DepthBufferSampler?.Dispose();
            DepthBufferSampler = GlStateManager.ResourceFactory.CreateSampler(depthBufferDescription);
        }

        SetFilterMode(SamplerFilter.MinPoint_MagPoint_MipPoint);
        colorSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
        colorSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
        ColorTextureSampler?.Dispose();
        ColorTextureSampler = GlStateManager.ResourceFactory.CreateSampler(colorSamplerDescription);
        
        var fbDesc = new FramebufferDescription(UseDepth ? DepthBufferId : null, ColorTextureId);
        FramebufferId?.Dispose();
        FramebufferId = GlStateManager.ResourceFactory.CreateFramebuffer(fbDesc);
        FramebufferId.Name = "Render Target";

        CheckStatus();
        Clear(clearError);
        UnbindRead();
    }

    public void Clear(bool clearError)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        BindWrite(true);

        var cl = GlStateManager.CommandList;
        cl.ClearColorTarget(0, new RgbaFloat(
            _clearChannels[RedChannel],
            _clearChannels[GreenChannel],
            _clearChannels[BlueChannel],
            _clearChannels[AlphaChannel]));

        if (UseDepth)
        {
            cl.ClearDepthStencil(1f);
        }
        
        UnbindWrite();
    }

    public void BindWrite(bool resetViewport) =>
        RenderSystem.EnsureOnRenderThread(() =>
            InternalBindWrite(resetViewport));

    private void InternalBindWrite(bool resetViewport)
    {
        RenderSystem.AssertOnRenderThreadOrInit();

        var cl = GlStateManager.CommandList;
        cl.SetFramebuffer(FramebufferId);
        
        if (resetViewport)
        {
            cl.SetViewport(0, new Viewport(0, 0, ViewWidth, ViewHeight, 0, 1));
        }
    }

    public void CheckStatus()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        // var status = GlStateManager.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        // if (status != FramebufferErrorCode.FramebufferComplete)
        //     throw new Exception($"{status}");
    }

    public void SetFilterMode(SamplerFilter filter)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        FilterMode = filter;
        
        colorSamplerDescription.Filter = filter;
        ColorTextureSampler?.Dispose();
        ColorTextureSampler = GlStateManager.ResourceFactory.CreateSampler(colorSamplerDescription);
    }

    public void UnbindRead()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
    }

    public void SetClearColor(float red, float green, float blue, float alpha)
    {
        _clearChannels[RedChannel] = red;
        _clearChannels[GreenChannel] = green;
        _clearChannels[BlueChannel] = blue;
        _clearChannels[AlphaChannel] = alpha;
    }

    public void BlitToScreen(int width, int height, bool noBlend = true)
    {
        RenderSystem.AssertOnGameThreadOrInit();
        RenderSystem.EnsureInInitPhase(() =>
            GlStateManager.WrapWithDebugGroup("RenderTarget::InternalBlitToScreen",
                () => InternalBlitToScreen(width, height, noBlend)
            )
        );
    }

    private void InternalBlitToScreen(int width, int height, bool noBlend)
    {
        RenderSystem.AssertOnRenderThread();
        GlStateManager.ColorMask(true, true, true, false);
        GlStateManager.DisableDepthTest();
        GlStateManager.DepthMask(false);
        GlStateManager.Viewport(0, 0, width, height);
        if (noBlend) GlStateManager.DisableBlend();

        var game = Game.Instance;
        var shader = game.GameRenderer.BlitShader;
        shader.SetSampler("DiffuseSampler", ColorTextureId);

        var matrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, 1000, 3000);
        RenderSystem.SetProjectionMatrix(matrix, IVertexSorting.OrthoZ);

        shader.ModelViewMatrix?.Set(Matrix4.CreateTranslation(0, 0, -2000));
        shader.ProjectionMatrix?.Set(matrix);
        shader.Apply();

        var f = (float) width;
        var g = (float) height;
        var h = (float) ViewWidth / Width;
        var k = (float) ViewHeight / Height;

        var tesselator = RenderSystem.RenderThreadTesselator;
        var builder = tesselator.Builder;
        builder.Begin(VertexFormat.Mode.Quads, DefaultVertexFormat.PositionTexColor);
        builder.Vertex(0, g, 0).Uv(0, 0).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(f, g, 0).Uv(h, 0).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(f, 0, 0).Uv(h, k).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(0, 0, 0).Uv(0, k).Color(255, 255, 255, 255).EndVertex();
        BufferUploader.Draw(builder.End());

        shader.Clear();
        GlStateManager.DepthMask(true);
        GlStateManager.ColorMask(true, true, true, true);
    }
}