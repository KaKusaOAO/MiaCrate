using MiaCrate.Client.Fonts;

namespace MiaCrate.Client.UI;

public class AllMissingGlyphProvider : IGlyphProvider
{
    public ISet<int> GetSupportedGlyphs() => new HashSet<int>();
    public IGlyphInfo GetGlyph(int id) => SpecialGlyphs.Missing;
}