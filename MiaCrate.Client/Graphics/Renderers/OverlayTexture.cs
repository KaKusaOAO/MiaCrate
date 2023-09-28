using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;

namespace MiaCrate.Client.Graphics;

public class OverlayTexture : IDisposable
{
    private const int Size = 16;
    public const int NoWhiteU = 0;
    public const int RedOverlayV = 3;
    public const int WhiteOverlayV = 10;

    public static int NoOverlay { get; } = Pack(NoWhiteU, WhiteOverlayV);

    private readonly DynamicTexture _texture = new(Size, Size, false);

    public OverlayTexture()
    {
        var image = _texture.Pixels!;

        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Size; j++)
            {
                if (i < 8)
                {
                    image.SetPixelRgba(j, i, 0xb20000ff);
                }
                else
                {
                    var k = (byte) ((1 - j / 15f * 0.75f) * 255f);
                    image.SetPixelRgba(j, i, Rgba32.White.WithAlpha(k));
                }
            }
        }
        
        // RenderSystem.ActiveTexture(GlStateManager.TextureUnit + 1);
        // _texture.Bind();
        image.Upload(_texture, 0, 0, 0, 0, 0, image.Width, image.Height, false, true, false, false);
        // RenderSystem.ActiveTexture(GlStateManager.TextureUnit);
    }
    
    public static int Pack(int a, int b) => a | b << Size;
    
    public void Dispose()
    {
        _texture.Dispose();
    }

    public void SetupOverlayColor()
    {
        // RenderSystem.SetupOverlayColor(() => _texture.Id, Size);
    }

    public void TeardownOverlayColor()
    {
        RenderSystem.TeardownOverlayColor();
    }
}