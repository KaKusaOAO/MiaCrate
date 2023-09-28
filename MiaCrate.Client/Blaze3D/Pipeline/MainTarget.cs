using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using Mochi.Utils;
using Veldrid;

namespace MiaCrate.Client.Pipeline;

public class MainTarget : RenderTarget
{
    public const int DefaultWidth = 854;
    public const int DefaultHeight = 480;

    private static readonly Dimension _defaultDimensions = new(DefaultWidth, DefaultHeight);

    public MainTarget(int width, int height) : base(true)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        RenderSystem.EnsureOnRenderThread(() => CreateFramebuffer(width, height));
    }

    private void CreateFramebuffer(int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();

        // Allocate the framebuffer attachments
        var dimension = AllocateAttachments(width, height);

        // Successfully allocated attachments.
        // Setup color buffer
        colorSamplerDescription = new SamplerDescription
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint
        };
        ColorTextureSampler = GlStateManager.ResourceFactory.CreateSampler(colorSamplerDescription);
        
        depthBufferDescription = new SamplerDescription
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
        };
        DepthBufferSampler = GlStateManager.ResourceFactory.CreateSampler(depthBufferDescription);

        var fbDesc = new FramebufferDescription(DepthBufferId, ColorTextureId);
        FramebufferId = GlStateManager.ResourceFactory.CreateFramebuffer(fbDesc);

        ViewWidth = Width = dimension.Width;
        ViewHeight = Height = dimension.Height;

        CheckStatus();
    }

    private Dimension AllocateAttachments(int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();

        var attachmentState = AttachmentState.None;
        foreach (var dimension in Dimension.ListWithFallback(width, height))
        {
            attachmentState = AttachmentState.None;
            if (AllocateColorAttachment(dimension)) attachmentState |= AttachmentState.Color;
            if (AllocateDepthAttachment(dimension)) attachmentState |= AttachmentState.Depth;
            if (attachmentState == (AttachmentState.Color | AttachmentState.Depth)) return dimension;
        }

        throw new Exception($"Unrecoverable GL_OUT_OF_MEMORY (allocated attachments = {attachmentState})");
    }

    private bool AllocateColorAttachment(Dimension dimension)
    {
        RenderSystem.AssertOnRenderThreadOrInit();

        try
        {
            var desc = new TextureDescription((uint) dimension.Width, (uint) dimension.Height,
                0, 0, 0, PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget, TextureType.Texture2D);
            ColorTextureId = GlStateManager.ResourceFactory.CreateTexture(ref desc);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex);
            return false;
        }
    }

    private bool AllocateDepthAttachment(Dimension dimension)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        
        try
        {
            var desc = new TextureDescription((uint) dimension.Width, (uint) dimension.Height,
                0, 0, 0, PixelFormat.R16_Float,
                TextureUsage.DepthStencil, TextureType.Texture2D);
            DepthBufferId = GlStateManager.ResourceFactory.CreateTexture(ref desc);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex);
            return false;
        }
    }

    private record Dimension(int Width, int Height)
    {
        public static List<Dimension> ListWithFallback(int width, int height)
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            var k = RenderSystem.MaxSupportedTextureSize;
            return width > 0 && width <= k && height > 0 && height <= k
                ? new List<Dimension> {new(width, height), _defaultDimensions}
                : new List<Dimension> {_defaultDimensions};
        }
    }

    [Flags]
    private enum AttachmentState
    {
        None,
        Color,
        Depth
    }
}