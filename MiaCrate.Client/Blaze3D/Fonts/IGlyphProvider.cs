namespace MiaCrate.Client.Fonts;

public interface IGlyphProvider : IDisposable
{
    public IGlyphInfo? GetGlyph(int id) => null;
    public ISet<int> GetSupportedGlyphs();
}