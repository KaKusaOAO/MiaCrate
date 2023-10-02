using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using Mochi.Extensions;
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
    public TextureInstance? ColorTexture { get; } = new();
    
    public TextureInstance? DepthBuffer { get; } = new();
    
    public SamplerFilter FilterMode { get; set; }
    
    protected RenderTarget(bool useDepth)
    {
        UseDepth = useDepth;
    }

    public void UnbindWrite()
    {
        RenderSystem.EnsureOnRenderThread(() =>
        {
            var device = GlStateManager.Device;
            GlStateManager.BindOutput(device.SwapchainFramebuffer);
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

        var cl = GlStateManager.EnsureBufferCommandBegan();
        cl.CopyTexture(target.DepthBuffer!.Texture, DepthBuffer!.Texture);

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

        DepthBuffer?.Dispose();
        ColorTexture?.Dispose();
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

        if (UseDepth)
        {
            var depthTextureDesc = TextureDescription.Texture2D((uint) Width, (uint) Height, 1, 1,
                PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil);

            var depthBufferDescription = new SamplerDescription
            {
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp
            };

            DepthBuffer!.UpdateTextureDescription(depthTextureDesc);
            DepthBuffer!.UpdateSamplerDescription(depthBufferDescription);
        }
        
        var colorTextureDesc = TextureDescription.Texture2D((uint) Width, (uint) Height, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled);
        ColorTexture!.UpdateTextureDescription(colorTextureDesc);
        
        SetFilterMode(SamplerFilter.MinPoint_MagPoint_MipPoint);
        ColorTexture.ModifySampler((ref SamplerDescription s) =>
        {
            s.AddressModeU = SamplerAddressMode.Clamp;
            s.AddressModeV = SamplerAddressMode.Clamp;
        });

        ColorTexture.EnsureUpToDate();
        DepthBuffer?.EnsureUpToDate();

        var fbDesc = new FramebufferDescription(UseDepth ? DepthBuffer!.Texture : null, ColorTexture!.Texture);
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

        var cl = GlStateManager.EnsureFramebufferSet(FramebufferId!);
        // cl.Begin();
        // cl.SetFramebuffer(FramebufferId!);
        cl.ClearColorTarget(0, new RgbaFloat(
            _clearChannels[RedChannel],
            _clearChannels[GreenChannel],
            _clearChannels[BlueChannel],
            _clearChannels[AlphaChannel]));

        if (UseDepth)
        {
            cl.ClearDepthStencil(1f);
        }
        
        // cl.End();
        // GlStateManager.Device.SubmitCommands(cl);
        UnbindWrite();
    }

    public void BindWrite(bool resetViewport) =>
        RenderSystem.EnsureOnRenderThread(() =>
            InternalBindWrite(resetViewport));

    private void InternalBindWrite(bool resetViewport)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.BindOutput(FramebufferId!);
        
        if (resetViewport)
        {
            var cl = GlStateManager.EnsureCommandBegan();
            cl.SetFramebuffer(FramebufferId!);
            cl.SetViewport(0, new Viewport(0, 0, ViewWidth, ViewHeight, 0, 1));
            GlStateManager.SubmitCommands();
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
        
        ColorTexture!.ModifySampler((ref SamplerDescription desc) =>
        {
            desc.Filter = filter;
        });
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
        shader.SetSampler("DiffuseSampler", ColorTexture);

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

        var device = GlStateManager.Device;
        var top = device.IsUvOriginTopLeft ? 0 : k;
        var bottom = device.IsUvOriginTopLeft ? k : 0;
        
        builder.Vertex(0, g, 0).Uv(0, bottom).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(f, g, 0).Uv(h, bottom).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(f, 0, 0).Uv(h, top).Color(255, 255, 255, 255).EndVertex();
        builder.Vertex(0, 0, 0).Uv(0, top).Color(255, 255, 255, 255).EndVertex();
        BufferUploader.Draw(builder.End());

        shader.Clear();
        GlStateManager.DepthMask(true);
        GlStateManager.ColorMask(true, true, true, true);
    }
}