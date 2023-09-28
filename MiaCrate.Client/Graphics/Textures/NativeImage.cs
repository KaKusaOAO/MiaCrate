using System.Runtime.CompilerServices;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using SkiaSharp;
using Veldrid;

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

    public Rgba32 GetPixelRgba(int x, int y)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"GetPixelRgba() only works on RGBA images; have {Format}");
        
        if (IsOutsideBounds(x, y))
            throw new ArgumentException($"({x}, {y}) outside of image bounds ({Width}, {Height})");

        return _bitmap.GetPixel(x, y);
    }
    
    public void SetPixelRgba(int x, int y, Rgba32 color)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"GetPixelRgba() only works on RGBA images; have {Format}");
        
        if (IsOutsideBounds(x, y))
            throw new ArgumentException($"({x}, {y}) outside of image bounds ({Width}, {Height})");

        _bitmap.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, color.Alpha));
    }

    public NativeImage MappedCopy(Func<Argb32, Argb32> transform)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"Function application only works on RGBA images; have {Format}");

        var image = new NativeImage(Width, Height, false);
        var size = Width * Height;
        
        unsafe
        {
            var src = (Argb32*) _bitmap.GetPixels();
            var dst = (Argb32*) image._bitmap.GetPixels();

            for (var i = 0; i < size; i++)
            {
                *dst++ = transform(*src++);
            }
        }

        return image;
    }
    
    public void ApplyToAllPixels(Func<Argb32, Argb32> transform)
    {
        if (Format != FormatInfo.Rgba)
            throw new ArgumentException($"Function application only works on RGBA images; have {Format}");

        var size = Width * Height;
        
        unsafe
        {
            var ptr = (Argb32*) _bitmap.GetPixels();

            for (var i = 0; i < size; i++)
            {
                *ptr++ = transform(*ptr);
            }
        }
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

    private static void SetFilter(AbstractTexture texture, bool blur, bool mipmap)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        texture.SetFilter(blur, mipmap);
    }
    
    public void Upload(AbstractTexture texture, int level, int xOffset, int yOffset, bool dispose) =>
        Upload(texture, level, xOffset, yOffset, 0, 0, Width, Height, false, dispose);

    public void Upload(AbstractTexture texture, int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool mipmap, bool dispose) =>
        Upload(texture, level, xOffset, yOffset, skipPixels, skipRows, width, height, false, false, mipmap, dispose);

    public void Upload(AbstractTexture texture, int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool isBlur, bool isClamp, bool mipmap, bool dispose)
    {
        RenderSystem.EnsureOnRenderThreadOrInit(() => 
            InternalUpload(texture, level, xOffset, yOffset, skipPixels, skipRows, width, height, isBlur, isClamp, mipmap, dispose));
    }
    
    private void InternalUpload(AbstractTexture texture, int level, int xOffset, int yOffset, int skipPixels, int skipRows, int width,
        int height, bool isBlur, bool isClamp, bool mipmap, bool dispose)
    {
        try
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            SetFilter(texture, isBlur, mipmap);

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

            var device = GlStateManager.Device;
            var factory = GlStateManager.ResourceFactory;
            device.UpdateTexture(texture.Texture, pixels, (uint) xOffset, (uint) yOffset, 0, 
                (uint) width, (uint) height, 0, (uint) level, 0);

            if (isClamp)
            {
                texture._samplerDescription.AddressModeU = SamplerAddressMode.Clamp;
                texture._samplerDescription.AddressModeV = SamplerAddressMode.Clamp;
                texture.Sampler?.Dispose();
                texture.Sampler = factory.CreateSampler(texture._samplerDescription);
            }
        }
        finally
        {
            if (dispose) Dispose();
        }
    }

    public void DownloadTexture(Texture texture, int level, bool noAlpha)
    {
        RenderSystem.AssertOnRenderThread();
        
        var device = GlStateManager.Device;
        var mapped = device.Map(texture, MapMode.Read);
        var dest = _bitmap.GetPixels();

        unsafe
        {
            var src = (int*) mapped.Data;
            var dst = (int*) dest;
            
            for (var i = 0; i < mapped.SizeInBytes; i++)
            {
                *dst++ = *src++;
            }
        }
        
        device.Unmap(texture);

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

    public void WriteToFile(string path)
    {
        using var stream = File.OpenWrite(path);
        _bitmap.Encode(stream, SKEncodedImageFormat.Png, 10);
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }
    
    public sealed class FormatInfo
    {
        public static readonly FormatInfo Rgba = 
            new(4, PixelFormat.R8_G8_B8_A8_UNorm, true, true, true, false, true, 0, 8, 16, 255, 24);
        public static readonly FormatInfo Rgb = 
            new(3, PixelFormat.BC1_Rgb_UNorm, true, true, true, false, false, 0, 8, 16, 255, 255);
        public static readonly FormatInfo LuminanceAlpha = 
            new(2, PixelFormat.R8_UNorm, false, false, false, false, true, 255, 255, 255, 0, 8);
        public static readonly FormatInfo Luminance = 
            new(1, PixelFormat.R8_UNorm, false, false, false, false, false, 0, 0, 0, 0, 255);
        
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
}