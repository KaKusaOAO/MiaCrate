using System.Runtime.CompilerServices;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace MiaCrate.Client.Graphics;

public sealed class NativeImage : IDisposable
{
    private SKBitmap _bitmap;
    public FormatInfo Format { get; }
    public int Width { get; }
    public int Height { get; }

    public NativeImage(int width, int height, bool clearBuffer) : this(FormatInfo.Rgba, width, height, clearBuffer)
    {
        
    }

    public NativeImage(FormatInfo format, int width, int height, bool clearBuffer)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException($"Invalid texture size: {width}x{height}");

        Format = format;
        Width = width;
        Height = height;

        var colorType = format.ToColorType();
        _bitmap = new SKBitmap(Width, Height, colorType, SKAlphaType.Unpremul);
    }

    private NativeImage(FormatInfo format, int width, int height, bool clearBuffer, SKBitmap bitmap)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException($"Invalid texture size: {width}x{height}");
        
        Format = format;
        Width = width;
        Height = height;
        _bitmap = bitmap;
    }

    private bool IsOutsideBounds(int x, int y) => x < 0 || x >= Width || y < 0 || y >= Height;

    public static NativeImage Read(byte[] arr)
    {
        var stream = new MemoryStream(arr);
        return Read(stream);
    }

    public static NativeImage Read(Stream stream) => Read(FormatInfo.Rgba, stream);

    public static NativeImage Read(FormatInfo? format, Stream stream)
    {
        var bitmap = SKBitmap.Decode(stream);
        return new NativeImage(
            format ?? FormatInfo.GetFormatFromByteCount(bitmap.BytesPerPixel), 
            bitmap.Width, bitmap.Height, true, bitmap);
    }

    public int GetPixelRgba(int x, int y)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"GetPixelRgba() only works on RGBA images; have {Format}");
        
        if (IsOutsideBounds(x, y))
            throw new ArgumentException($"({x}, {y}) outside of image bounds ({Width}, {Height})");

        var color = _bitmap.GetPixel(x, y);
        var arr = new[]
        {
            color.Red, color.Green, color.Blue, color.Alpha
        };
        
        return BitConverter.ToInt32(arr, 0);
    }
    
    public void SetPixelRgba(int x, int y, int color)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"GetPixelRgba() only works on RGBA images; have {Format}");
        
        if (IsOutsideBounds(x, y))
            throw new ArgumentException($"({x}, {y}) outside of image bounds ({Width}, {Height})");

        var arr = BitConverter.GetBytes(color);
        _bitmap.SetPixel(x, y, new SKColor(arr[0], arr[1], arr[2], arr[3]));
    }

    public NativeImage MappedCopy(Func<int, int> transform)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"Function application only works on RGBA images; have {Format}");

        var image = new NativeImage(Width, Height, false);
        var size = Width * Height;
        
        unsafe
        {
            var src = (int*) _bitmap.GetPixels();
            var dst = (int*) image._bitmap.GetPixels();

            for (var i = 0; i < size; i++)
            {
                *dst++ = transform(*src++);
            }
        }

        return image;
    }

    public byte GetLuminanceOrAlpha(int x, int y)
    {
        if (!Format.HasLuminanceOrAlpha)
            throw new ArgumentException($"No luminance or alpha in {Format}");
        
        if (IsOutsideBounds(x, y))
            throw new ArgumentException($"({x}, {y}) outside of image bounds ({Width}, {Height})");

        var offset = (x + y * Width) * Format.Components + Format.LuminanceOrAlphaOffset / 8;
        
        unsafe
        {
            var ptr = (byte*) (_bitmap.GetPixels() + offset);
            return *ptr;
        }
    }

    private static void SetFilter(bool blur, bool mipmap)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        if (blur)
        {
            GlStateManager.TexMinFilter(TextureTarget.Texture2D, 
                mipmap ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
            GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Linear);
        }
        else
        {
            GlStateManager.TexMinFilter(TextureTarget.Texture2D, 
                mipmap ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest);
            GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Nearest);
        }
    }
    
    public void Upload(int level, int xOffset, int yOffset, bool dispose) =>
        Upload(level, xOffset, yOffset, 0, 0, Width, Height, false, dispose);

    public void Upload(int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool mipmap, bool dispose) =>
        Upload(level, xOffset, yOffset, skipPixels, skipRows, width, height, false, false, mipmap, dispose);

    public void Upload(int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool isBlur, bool isClamp, bool mipmap, bool dispose)
    {
        RenderSystem.EnsureOnRenderThreadOrInit(() => 
            InternalUpload(level, xOffset, yOffset, skipPixels, skipRows, width, height, isBlur, isClamp, mipmap, dispose));
    }
    
    private void InternalUpload(int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool isBlur, bool isClamp, bool mipmap, bool dispose)
    {
        try
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            SetFilter(isBlur, mipmap);
            if (width == Width)
            {
                GlStateManager.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            }
            else
            {
                GlStateManager.PixelStore(PixelStoreParameter.UnpackRowLength, Width);
            }

            GlStateManager.PixelStore(PixelStoreParameter.UnpackSkipPixels, skipPixels);
            GlStateManager.PixelStore(PixelStoreParameter.UnpackSkipRows, skipRows);
            Format.SetUnpackPixelStoreState();

            // SKColor is stored in ARGB format, but the buffer data required that int to be ABGR,
            // which is the reverse of RGBA, the order represents in the buffer.
            
            // Rearrange the pixel to the correct order: ARGB => ABGR (reversed form of RGBA)
            
            var pixels = Enumerable.Range(0, _bitmap.Height)
                .Select(y => Enumerable.Range(0, _bitmap.Width)
                    .Select(x => (x, y)))
                .SelectMany(e => 
                    e.Select(n => _bitmap.GetPixel(n.x, n.y))
                )
                .Select(c => new Rgba32(c.Red, c.Green, c.Blue, c.Alpha).RGBA)
                .ToArray();

            GlStateManager.TexSubImage2D(TextureTarget.Texture2D, level, xOffset, yOffset, width, height, Format.Format,
                PixelType.UnsignedByte, pixels);

            if (isClamp)
            {
                GlStateManager.TexWrapS(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
                GlStateManager.TexWrapT(TextureTarget.Texture2D, TextureWrapMode.ClampToEdge);
            }
        }
        finally
        {
            if (dispose) Dispose();
        }
    }

    public void DownloadTexture(int level, bool noAlpha)
    {
        RenderSystem.AssertOnRenderThread();
        Format.SetPackPixelStoreState();
        GlStateManager.GetTexImage(TextureTarget.Texture2D, level, Format.Format, PixelType.UnsignedByte, _bitmap.GetPixels());

        if (noAlpha && Format.HasAlpha)
        {
            var size = Width * Height;
            
            unsafe
            {
                var ptr = (int*) _bitmap.GetPixels();
                for (var i = 0; i < size; i++)
                {
                    var color = *ptr;
                    *ptr++ = color | 0xff << Format.AlphaOffset;
                }
            }
        }
    }

    public sealed class FormatInfo
    {
        public static readonly FormatInfo Rgba = 
            new(4, PixelFormat.Rgba, true, true, true, false, true, 0, 8, 16, 255, 24);
        public static readonly FormatInfo Rgb = 
            new(3, PixelFormat.Rgb, true, true, true, false, false, 0, 8, 16, 255, 255);
        public static readonly FormatInfo LuminanceAlpha = 
            new(2, PixelFormat.LuminanceAlpha, false, false, false, false, true, 255, 255, 255, 0, 8);
        public static readonly FormatInfo Luminance = 
            new(1, PixelFormat.Luminance, false, false, false, false, false, 0, 0, 0, 0, 255);
        
        public int Components { get; }
        public PixelFormat Format { get; }
        public bool HasRed { get; }
        public bool HasGreen { get; }
        public bool HasBlue { get; }
        public bool HasLuminance { get; }
        public bool HasAlpha { get; }
        public int RedOffset { get; }
        public int GreenOffset { get; }
        public int BlueOffset { get; }
        public int LuminanceOffset { get; }
        public int AlphaOffset { get; }
        public string Name { get; }

        public bool HasLuminanceOrRed => HasLuminance || HasRed;
        public bool HasLuminanceOrGreen => HasLuminance || HasGreen;
        public bool HasLuminanceOrBlue => HasLuminance || HasBlue;
        public bool HasLuminanceOrAlpha => HasLuminance || HasAlpha;

        public int LuminanceOrRedOffset => HasLuminance ? LuminanceOffset : RedOffset;
        public int LuminanceOrGreenOffset => HasLuminance ? LuminanceOffset : GreenOffset;
        public int LuminanceOrBlueOffset => HasLuminance ? LuminanceOffset : BlueOffset;
        public int LuminanceOrAlphaOffset => HasLuminance ? LuminanceOffset : AlphaOffset;

        private FormatInfo(int components, PixelFormat format, bool hasRed, bool hasGreen, bool hasBlue,
            bool hasLuminance, bool hasAlpha, int redOffset, int greenOffset, int blueOffset, int luminanceOffset,
            int alphaOffset, [CallerMemberName] string name = "")
        {
            Components = components;
            Format = format;
            HasRed = hasRed;
            HasGreen = hasGreen;
            HasBlue = hasBlue;
            HasLuminance = hasLuminance;
            HasAlpha = hasAlpha;
            RedOffset = redOffset;
            GreenOffset = greenOffset;
            BlueOffset = blueOffset;
            LuminanceOffset = luminanceOffset;
            AlphaOffset = alphaOffset;
            Name = name;
        }

        public void SetUnpackPixelStoreState()
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            GlStateManager.PixelStore(PixelStoreParameter.UnpackAlignment, Components);
        }

        public void SetPackPixelStoreState()
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            GlStateManager.PixelStore(PixelStoreParameter.PackAlignment, Components);
        }

        public static FormatInfo GetFormatFromByteCount(int count)
        {
            return count switch
            {
                1 => Luminance,
                2 => LuminanceAlpha,
                3 => Rgb,
                4 => Rgba,
                _ => Rgba
            };
        }
        
        public SKColorType ToColorType()
        {
            return Components switch {
                1 => SKColorType.Gray8,
                2 => SKColorType.Rg88,
                3 => SKColorType.Rgb888x,
                4 => SKColorType.Rgba8888,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override string ToString() => Name;
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }
}