namespace MiaCrate.Client.Fonts;

public interface IGlyphInfo
{
    public float Advance { get; }
    public float GetAdvance(bool isBold) => Advance + (isBold ? BoldOffset : 0f);
    public float BoldOffset => 1f;
    public float ShadowOffset => 1f;
}