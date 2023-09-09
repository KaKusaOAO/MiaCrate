namespace MiaCrate.Client.Graphics;

public static class MissingTextureAtlasSprite
{
    private const int MissingImageWidth = 16;
    private const int MissingImageHeight = 16;
    private const string MissingTextureName = "missingno";

    private static readonly ResourceLocation MissingTextureLocation = new(MissingTextureName);
    private static DynamicTexture? _missingTexture;

    private static NativeImage GenerateMissingImage(int width, int height)
    {
        var image = new NativeImage(width, height, false);
        const uint black = 0xff000000;
        const uint magenta = 0xfff800f8;

        for (var m = 0; m < height; m++)
        {
            for (var n = 0; n < width; n++)
            {
                if (m < height / 2 ^ n < width / 2)
                {
                    image.SetPixelRgba(n, m, unchecked((int) magenta));
                }
                else
                {
                    image.SetPixelRgba(n, m, unchecked((int) black));
                }
            }
        }

        return image;
    }

    public static DynamicTexture Texture
    {
        get
        {
            if (_missingTexture != null) 
                return _missingTexture;
            
            var image = GenerateMissingImage(MissingImageWidth, MissingImageHeight);
            _missingTexture = new DynamicTexture(image);
            Game.Instance.TextureManager.Register(MissingTextureLocation, _missingTexture);
            return _missingTexture;
        }
    }

    public static ResourceLocation Location => MissingTextureLocation;
}