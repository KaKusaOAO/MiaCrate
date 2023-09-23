using MiaCrate.Client.UI;

namespace MiaCrate.Client.Fonts;

public interface IGlyphInfo
{
    public float Advance { get; }
    public float BoldOffset => 1f;
    public float ShadowOffset => 1f;
    public float GetAdvance(bool isBold) => Advance + (isBold ? BoldOffset : 0f);

    public BakedGlyph Bake(Func<ISheetGlyphInfo, BakedGlyph> func);

    public interface ISpace : IGlyphInfo
    {
        public static ISpace Create(float advance) => new Instance(advance); 

        private class Instance : ISpace
        {
            public float Advance { get; }
            
            public Instance(float advance)
            {
                Advance = advance;
            }
        }
        
        BakedGlyph IGlyphInfo.Bake(Func<ISheetGlyphInfo, BakedGlyph> func) => EmptyGlyph.Instance;
    }
}