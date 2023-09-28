using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;

namespace MiaCrate.Client.UI;

public sealed class SpecialGlyphs : IGlyphInfo
{
    public static readonly SpecialGlyphs White = new(() => Generate(5, 8, (_, _) => -1));
    public static readonly SpecialGlyphs Missing = new(() => Generate(5, 8, (x, y) =>
    {
        var white = x == 0 || x + 1 == 5 || y == 0 || y + 1 == 8;
        return white ? -1 : 0;
    }));

    private readonly NativeImage _image;

    private SpecialGlyphs(Func<NativeImage> supplier)
    {
        _image = supplier();
    }

    private static NativeImage Generate(int width, int height, Func<int, int, int> color)
    {
        var image = new NativeImage(NativeImage.FormatInfo.Rgba, width, height, false);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                image.SetPixelRgba(x, y, color(x, y));
            }
        }

        return image;
    }

    public float Advance => _image.Width + 1;
    
    public BakedGlyph Bake(Func<ISheetGlyphInfo, BakedGlyph> func) => func(new Info(this));

    private class Info : ISheetGlyphInfo
    {
        private readonly SpecialGlyphs _instance;

        public Info(SpecialGlyphs instance)
        {
            _instance = instance;
        }

        public int PixelWidth => _instance._image.Width;

        public int PixelHeight => _instance._image.Height;

        public bool IsColored => true;

        public float Oversample => 1;

        public void Upload(AbstractTexture texture, int x, int y)
        {
            _instance._image.Upload(texture, 0, x, y, false);
        }
    }
}