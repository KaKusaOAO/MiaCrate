using MiaCrate.Client.Graphics;
using MiaCrate.Client.Utils;
using MiaCrate.Extensions;
using MiaCrate.Localizations;
using Mochi.Texts;
using OpenTK.Mathematics;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Client.UI;

public class Font
{
    private const float EffectDepth = 0.01f;
    private static readonly Vector3 _shadowOffset = new(0, 0, 0.03f);

    public const int AlphaCutoff = 8;
    public const int LineHeight = 9;
    
    private readonly Func<ResourceLocation, FontSet> _fonts;
    private readonly bool _filterFishyGlyphs;
    private readonly StringSplitter _splitter;
    
    public bool IsBidirectional => Language.Instance.IsDefaultRightToLeft;
    
    public Font(Func<ResourceLocation, FontSet> fonts, bool filterFishyGlyphs)
    {
        _fonts = fonts;
        _filterFishyGlyphs = filterFishyGlyphs;
        _splitter = new StringSplitter((i, style) =>
            GetFontSet(style.Font).GetGlyphInfo(i, _filterFishyGlyphs).GetAdvance(style.IsBold));
    }

    private FontSet GetFontSet(ResourceLocation location) => _fonts(location);

    public int DrawInBatch(string str, float x, float y, Argb32 color, bool drawShadow, ref Matrix4 matrix,
        IMultiBufferSource source, DisplayMode mode, Argb32 effectColor, int packedLightCoords) =>
        DrawInBatch(str, x, y, color, drawShadow, ref matrix, source, mode, effectColor, packedLightCoords, IsBidirectional);

    public int DrawInBatch(string str, float x, float y, Argb32 color, bool drawShadow, ref Matrix4 matrix,
        IMultiBufferSource source, DisplayMode mode, Argb32 effectColor, int packedLightCoords, bool bidi) =>
        DrawInternal(str, x, y, color, drawShadow, ref matrix, source, mode, effectColor, packedLightCoords, bidi);
    
    public int DrawInBatch(IComponent component, float f, float g, int i, bool bl, ref Matrix4 matrix,
        IMultiBufferSource source, DisplayMode mode, int j, int packedLightCoords) =>
        DrawInBatch(component.GetVisualOrderText(), f, g, i, bl, ref matrix, source, mode, j, packedLightCoords);
    
    public int DrawInBatch(FormattedCharSequence seq, float f, float g, int i, bool bl, ref Matrix4 matrix,
        IMultiBufferSource source, DisplayMode mode, int j, int packedLightCoords) =>
        DrawInternal(seq, f, g, i, bl, ref matrix, source, mode, j, packedLightCoords);

    private int DrawInternal(string str, float x, float y, Argb32 color, bool drawShadow, ref Matrix4 matrix, IMultiBufferSource source,
        DisplayMode mode, Argb32 effectColor, int packedLightCoords, bool bl2)
    {
        // bl2 = bidirectional related

        color = AdjustColor(color);
        var m2 = matrix;
        
        if (drawShadow)
        {
            RenderText(str, x, y, color, true, matrix, source, mode, effectColor, packedLightCoords);
            matrix *= Matrix4.CreateTranslation(_shadowOffset);
        }

        x = RenderText(str, x, y, color, false, m2, source, mode, effectColor, packedLightCoords);
        return (int) x + (drawShadow ? 1 : 0);
    }
    
    private int DrawInternal(FormattedCharSequence seq, float x, float y, Argb32 color, bool drawShadow, ref Matrix4 matrix, IMultiBufferSource source,
        DisplayMode mode, Argb32 effectColor, int packedLightCoords)
    {
        color = AdjustColor(color);
        var m2 = matrix;
        
        if (drawShadow)
        {
            RenderText(seq, x, y, color, true, matrix, source, mode, effectColor, packedLightCoords);
            matrix *= Matrix4.CreateTranslation(_shadowOffset);
        }

        x = RenderText(seq, x, y, color, false, m2, source, mode, effectColor, packedLightCoords);
        return (int) x + (drawShadow ? 1 : 0);
    }

    private Argb32 AdjustColor(Argb32 i) => (i & 0xfc000000) == 0 ? i | 0xff000000 : i;

    private float RenderText(string str, float x, float y, Argb32 color, bool dropShadow, Matrix4 matrix, IMultiBufferSource source,
        DisplayMode mode, Argb32 effectColor, int packedLightCoords)
    {
        var output = new StringRenderOutput(this, source, x, y, color, dropShadow, matrix, mode, packedLightCoords);
        StringDecomposer.IterateFormatted(str, Style.Empty, output);
        return output.Finish(effectColor, x);
    }
    
    private float RenderText(FormattedCharSequence seq, float x, float y, Argb32 color, bool dropShadow, Matrix4 matrix, IMultiBufferSource source,
        DisplayMode mode, Argb32 effectColor, int packedLightCoords)
    {
        var output = new StringRenderOutput(this, source, x, y, color, dropShadow, matrix, mode, packedLightCoords);
        seq(output);
        return output.Finish(effectColor, x);
    }

    private void RenderChar(BakedGlyph bakedGlyph, bool bold, bool italic, float boldOffset, float x, float y, Matrix4 matrix,
        IVertexConsumer consumer, float red, float green, float blue, float alpha, int packedLightCoords)
    {
        bakedGlyph.Render(italic, x, y, matrix, consumer, red, green, blue, alpha, packedLightCoords);
        
        if (!bold) return;
        bakedGlyph.Render(italic, x + boldOffset, y, matrix, consumer, red, green, blue, alpha, packedLightCoords);
    }

    public enum DisplayMode
    {
        Normal,
        SeeThrough,
        PolygonOffset
    }
    
    private class StringRenderOutput : IFormattedCharSink
    {
        private readonly Font _font;
        private readonly IMultiBufferSource _bufferSource;
        private readonly bool _dropShadow;
        private readonly Matrix4 _pose;
        private readonly DisplayMode _mode;
        private readonly int _packedLightCoords;
        private readonly float _dimFactor;
        private readonly float _r;
        private readonly float _g;
        private readonly float _b;
        private readonly float _a;
        private List<BakedGlyph.Effect>? _effects;
        
        public float X { get; set; }
        public float Y { get; set; }

        public StringRenderOutput(Font font, IMultiBufferSource bufferSource, float x, float y, Argb32 color, bool dropShadow,
            Matrix4 pose, DisplayMode mode, int packedLightCoords)
        {
            _font = font;
            _bufferSource = bufferSource;
            X = x;
            Y = y;
            _dropShadow = dropShadow;
            _dimFactor = dropShadow ? 0.25f : 1f;
            _r = color.Red / 255f * _dimFactor;
            _g = color.Green / 255f * _dimFactor;
            _b = color.Blue / 255f * _dimFactor;
            _a = color.Alpha / 255f * _dimFactor;
            _pose = pose;
            _mode = mode;
            _packedLightCoords = packedLightCoords;
        }

        public bool Accept(int i, Style style, int codepoint)
        {
            var fontSet = _font.GetFontSet(style.Font);
            var info = fontSet.GetGlyphInfo(codepoint, _font._filterFishyGlyphs);
            var bakedGlyph = style.IsObfuscated && codepoint != 32
                ? fontSet.GetRandomGlyph(info)
                : fontSet.GetGlyph(codepoint);

            var bl = style.IsBold;
            
            var f = _a;
            float g;
            float h;
            float l;
            
            var color = style.Color;
            if (color != null)
            {
                var k = color.Color;
                g = k.R / 255f * _dimFactor;
                h = k.G / 255f * _dimFactor;
                l = k.B / 255f * _dimFactor;
            }
            else
            {
                g = _r;
                h = _g;
                l = _b;
            }

            if (bakedGlyph is not EmptyGlyph)
            {
                var boldOffset = bl ? info.BoldOffset : 0;
                var shadowOffset = _dropShadow ? info.ShadowOffset : 0;
                var consumer = _bufferSource.GetBuffer(bakedGlyph.RenderType(_mode));
                _font.RenderChar(bakedGlyph, bl, style.IsItalic, boldOffset, X + shadowOffset, Y + shadowOffset, _pose, consumer, g, h, l, f,
                    _packedLightCoords);
            }

            var m = info.GetAdvance(bl);
            var n = _dropShadow ? 1f : 0;
            if (style.IsStrikethrough)
                AddEffect(new BakedGlyph.Effect(X + n - 1, Y + n + 4.5f, X + n + m, Y + n + 4.5f - 1f, EffectDepth, g, h, l, f));

            if (style.IsUnderlined)
                AddEffect(new BakedGlyph.Effect(X + n - 1, Y + n + LineHeight, X + n + m, Y + n + LineHeight - 1f, EffectDepth, g, h, l, f));

            X += m;
            return true;
        }

        private void AddEffect(BakedGlyph.Effect effect)
        {
            _effects ??= new List<BakedGlyph.Effect>();
            _effects.Add(effect);
        }

        public float Finish(Argb32 color, float x)
        {
            if (color != 0)
            {
                var a = color.Alpha / 255f;
                var r = color.Red / 255f;
                var g = color.Green / 255f;
                var b = color.Blue / 255f;
                AddEffect(new BakedGlyph.Effect(x - 1f, Y + LineHeight, X + 1f, Y - 1f, EffectDepth, r, g, b, a));
            }

            if (_effects != null)
            {
                var glyph = _font.GetFontSet(Style.DefaultFont).WhiteGlyph;
                var consumer = _bufferSource.GetBuffer(glyph.RenderType(_mode));
                
                foreach (var effect in _effects)
                {
                    glyph.RenderEffect(effect, _pose, consumer, _packedLightCoords);
                }
            }

            return X;
        }
    }
}