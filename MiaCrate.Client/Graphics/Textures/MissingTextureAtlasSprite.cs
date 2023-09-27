using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.Data;
using MiaCrate.Resources;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Graphics;

public static class MissingTextureAtlasSprite
{
    private const int MissingImageWidth = 16;
    private const int MissingImageHeight = 16;
    private const string MissingTextureName = "missingno";
    private static readonly ResourceLocation _missingTextureLocation = new(MissingTextureName);
    private static readonly IResourceMetadata _spriteMetadata =
        new IResourceMetadata.Builder()
            .Put(AnimationMetadataSection.Serializer, new AnimationMetadataSection(new List<AnimationFrame>()
            {
                new AnimationFrame(0, AnimationFrame.UnknownFrameTime)
            }, MissingImageWidth, MissingImageHeight, 1, false))
            .Build();
    
    private static DynamicTexture? _missingTexture;

    public static DynamicTexture Texture
    {
        get
        {
            if (_missingTexture != null) 
                return _missingTexture;
            
            var image = GenerateMissingImage(MissingImageWidth, MissingImageHeight);
            _missingTexture = new DynamicTexture(image);
            GlStateManager.ObjectLabel(ObjectLabelIdentifier.Texture, _missingTexture.Id, "MissingTexture");
            Game.Instance.TextureManager.Register(_missingTextureLocation, _missingTexture);
            return _missingTexture;
        }
    }

    public static ResourceLocation Location => _missingTextureLocation;

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
    
    public static SpriteContents Create()
    {
        var image = GenerateMissingImage(MissingImageWidth, MissingImageHeight);
        return new SpriteContents(_missingTextureLocation, new FrameSize(MissingImageWidth, MissingImageHeight), 
            image, _spriteMetadata);
    }
}