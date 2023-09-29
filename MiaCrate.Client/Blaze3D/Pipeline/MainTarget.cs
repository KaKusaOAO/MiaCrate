using System.Drawing;
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
        ColorTexture!.ModifySampler((ref SamplerDescription desc) =>
        {
            desc.AddressModeU = SamplerAddressMode.Clamp;
            desc.AddressModeW = SamplerAddressMode.Clamp;
            desc.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
        });
        
        DepthBuffer!.ModifySampler((ref SamplerDescription desc) =>
        {
            desc.AddressModeU = SamplerAddressMode.Clamp;
            desc.AddressModeW = SamplerAddressMode.Clamp;
            desc.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
        });

        var fbDesc = new FramebufferDescription(DepthBuffer!.Texture, ColorTexture!.Texture);
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
            var desc = TextureDescription.Texture2D((uint) dimension.Width, (uint) dimension.Height,
                1, 1, PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled);
            ColorTexture!.UpdateTextureDescription(desc);            
            ColorTexture.EnsureTextureUpToDate();
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
            var desc = TextureDescription.Texture2D((uint) dimension.Width, (uint) dimension.Height,
                1, 1, PixelFormat.D24_UNorm_S8_UInt,
                TextureUsage.DepthStencil);
            DepthBuffer!.UpdateTextureDescription(desc);
            DepthBuffer.EnsureTextureUpToDate();
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