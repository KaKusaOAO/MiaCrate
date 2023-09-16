using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using Mochi.Brigadier;
using OpenTK.Graphics.OpenGL4;

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
        // Then we create a new framebuffer and bind it.
        FramebufferId = GlStateManager.GenFramebuffers();
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);
        
        // Setup color buffer
        GlStateManager.BindTexture(ColorTextureId);
        GlStateManager.TexMinFilter(TextureTarget.Texture2D, TextureMinFilter.Nearest);
        GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Nearest);
        GlStateManager.TexWrapS(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.TexWrapT(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorTextureId, 0);
        
        // Setup depth buffer
        GlStateManager.BindTexture(DepthBufferId);
        GlStateManager.TexCompareMode(TextureTarget.Texture2D, TextureCompareMode.None);
        GlStateManager.TexMinFilter(TextureTarget.Texture2D, TextureMinFilter.Nearest);
        GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Nearest);
        GlStateManager.TexWrapS(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.TexWrapT(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
        GlStateManager.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthBufferId, 0);
        
        // Unbind the texture
        GlStateManager.BindTexture(0);

        ViewWidth = Width = dimension.Width;
        ViewHeight = Height = dimension.Height;
        
        CheckStatus();
        GlStateManager.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private Dimension AllocateAttachments(int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        ColorTextureId = TextureUtil.GenerateTextureId();
        DepthBufferId = TextureUtil.GenerateTextureId();

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
        GlStateManager.GetError();
        GlStateManager.BindTexture(ColorTextureId);
        GlStateManager.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 
            dimension.Width, dimension.Height, 0, 
            PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        return GlStateManager.GetError() != ErrorCode.OutOfMemory;
    }
    
    private bool AllocateDepthAttachment(Dimension dimension)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.GetError();
        GlStateManager.BindTexture(DepthBufferId);
        GlStateManager.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 
            dimension.Width, dimension.Height, 0, 
            PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        return GlStateManager.GetError() != ErrorCode.OutOfMemory;
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