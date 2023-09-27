using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public static class TextureUtil
{
    public static int GenerateTextureId()
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        if (!SharedConstants.IsRunningInIde) return GlStateManager.GenTexture();
        
        var iArr = GlStateManager.GenTextures(Random.Shared.Next(15) + 1);
        var i = GlStateManager.GenTexture();
        GlStateManager.DeleteTextures(iArr);
        return i;
    }

    public static void ReleaseTextureId(int texture)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.DeleteTexture(texture);
    }

    public static void PrepareImage(int texture, int width, int height) =>
        PrepareImage(PixelInternalFormat.Rgba, texture, 0, width, height);
    
    public static void PrepareImage(PixelInternalFormat format, int texture, int width, int height) =>
        PrepareImage(format, texture, 0, width, height);

    public static void PrepareImage(int texture, int maxLevel, int width, int height) =>
        PrepareImage(PixelInternalFormat.Rgba, texture, maxLevel, width, height);

    public static void PrepareImage(PixelInternalFormat format, int texture, int maxLevel, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        Bind(texture);

        if (maxLevel >= 0)
        {
            GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, maxLevel);
            GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, 0);
            GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, maxLevel);
            GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, 0f);
        }

        for (var m = 0; m <= maxLevel; m++)
        {
            GlStateManager.TexImage2D(TextureTarget.Texture2D, m, format, width >> m, height >> m, 0, 
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }
    }

    private static void Bind(int texture)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        GlStateManager.BindTexture(texture);
    }

    public static void WriteAsPng(string path, string str, int id, int j, int k, int l, Func<Argb32, Argb32>? transform)
    {
        RenderSystem.AssertOnRenderThread();
        Bind(id);

        for (var m = 0; m <= j; m++)
        {
            var n = k >> m;
            var o = l >> m;

            try
            {
                using var image = new NativeImage(n, o, false);

                image.DownloadTexture(m, false);
                if (transform != null) image.ApplyToAllPixels(transform);

                var p = Path.Combine(path, $"{str}_{m}.png");
                image.WriteToFile(p);
                Logger.Verbose($"Exported png to: {Path.GetFullPath(p)}");
            }
            catch (Exception ex)
            {
                Logger.Verbose("Unable to write: ");
                Logger.Verbose(ex);
            }
        }
    }
}