using MiaCrate.Client.Fonts;

namespace MiaCrate.Client.Graphics;

public class FontSet : IDisposable
{
    private readonly TextureManager _textureManager;
    private readonly ResourceLocation _name;

    public FontSet(TextureManager textureManager, ResourceLocation name)
    {
        _textureManager = textureManager;
        _name = name;
    }

    public void Reload(List<IGlyphProvider> providers)
    {
        
    }
    
    public void Dispose()
    {
    }
}