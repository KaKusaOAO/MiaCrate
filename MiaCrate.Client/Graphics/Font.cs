namespace MiaCrate.Client.Graphics;

public class Font
{
    private readonly Func<ResourceLocation, FontSet> _fonts;
    private readonly bool _filterFishyGlyphs;

    public Font(Func<ResourceLocation, FontSet> fonts, bool filterFishyGlyphs)
    {
        _fonts = fonts;
        _filterFishyGlyphs = filterFishyGlyphs;
    }
}