using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using Mochi.Utils;
using Veldrid;

namespace MiaCrate.Client.Platform;

public static class TextureUtil
{
    public static TexturePreparation PrepareImage(int width, int height) =>
        PrepareImage(PixelFormat.R8_G8_B8_A8_UNorm, 0, width, height);
    
    public static TexturePreparation PrepareImage(PixelFormat format, int width, int height) =>
        PrepareImage(format, 0, width, height);

    public static TexturePreparation PrepareImage(int maxLevel, int width, int height) =>
        PrepareImage(PixelFormat.R8_G8_B8_A8_UNorm, maxLevel, width, height);

    public static TexturePreparation PrepareImage(PixelFormat format, int maxLevel, int width, int height)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        
        var texDesc = new TextureDescription
        {
            Width = (uint) width,
            Height = (uint) height,
            Format = format,
            MipLevels = (uint) maxLevel,
            Usage = TextureUsage.Sampled
        };

        var sampleDesc = new SamplerDescription();

        if (maxLevel >= 0)
        {
            sampleDesc.MinimumLod = 0;
            sampleDesc.MaximumLod = (uint) maxLevel;
            sampleDesc.LodBias = 0;
        }

        // if (maxLevel >= 0)
        // {
        //     GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, maxLevel);
        //     GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, 0);
        //     GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, maxLevel);
        //     GlStateManager.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, 0f);
        // }
        //
        // for (var m = 0; m <= maxLevel; m++)
        // {
        //     GlStateManager.TexImage2D(TextureTarget.Texture2D, m, format, width >> m, height >> m, 0, 
        //         PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        // }
        
        return new TexturePreparation(texDesc, sampleDesc);
    }

    public record TexturePreparation(TextureDescription TextureDescription, SamplerDescription SamplerDescription);

    public static void WriteAsPng(Texture texture, string path, string str, int j, int k, int l, Func<Argb32, Argb32>? transform)
    {
        RenderSystem.AssertOnRenderThread();

        for (var m = 0; m <= j; m++)
        {
            var n = k >> m;
            var o = l >> m;

            try
            {
                using var image = new NativeImage(n, o, false);

                image.DownloadTexture(texture, m, false);
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