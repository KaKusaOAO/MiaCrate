using MiaCrate.Client.Graphics;
using MiaCrate.Resources;

namespace MiaCrate.Client.UI;

public class FontManager : IPreparableReloadListener, IDisposable
{
    private readonly TextureManager _textureManager;

    public FontManager(TextureManager textureManager)
    {
        _textureManager = textureManager;
    }

    public Font CreateFont()
    {
        throw new NotImplementedException();
    }
}