using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using Mochi.Extensions;

namespace MiaCrate.Client.UI;

public class FontSet : IDisposable
{
    private const float LargeForwardAdvance = 32;
    
    private static IRandomSource Random { get; } = IRandomSource.Create();

    private readonly TextureManager _textureManager;
    private readonly ResourceLocation _name;

    private BakedGlyph _missingGlyph;
    public BakedGlyph WhiteGlyph { get; private set; }
    private readonly List<IGlyphProvider> _providers = new();
    private readonly CodepointMap<BakedGlyph> _glyphs = new();
    private readonly CodepointMap<GlyphInfoFilter> _glyphInfos = new();
    private readonly Dictionary<int, List<int>> _glyphsByWidth = new();
    private readonly List<FontTexture> _textures = new();

    public FontSet(TextureManager textureManager, ResourceLocation name)
    {
        _textureManager = textureManager;
        _name = name;
    }

    public void Reload(List<IGlyphProvider> list)
    {
        CloseProviders();
        CloseTextures();
        
        _glyphs.Clear();
        _glyphInfos.Clear();
        _glyphsByWidth.Clear();

        _missingGlyph = SpecialGlyphs.Missing.Bake(Stitch);
        WhiteGlyph = SpecialGlyphs.White.Bake(Stitch);

        var glyphSet = new HashSet<int>();
        foreach (var item in list.SelectMany(provider => provider.GetSupportedGlyphs()))
        {
            glyphSet.Add(item);
        }

        var set = new HashSet<IGlyphProvider>();
        foreach (var i in glyphSet)
        {
            foreach (var provider in list)
            {
                var info = provider.GetGlyph(i);
                if (info == null) continue;

                set.Add(provider);
                if (info != SpecialGlyphs.Missing)
                {
                    _glyphsByWidth.ComputeIfAbsent((int) Math.Ceiling(info.GetAdvance(false)), 
                        _ => new List<int>());
                }

                break;
            }
        }
        
        foreach (var provider in list.Where(p => set.Contains(p)))
        {
            _providers.Add(provider);
        }
    }

    private GlyphInfoFilter ComputeGlyphInfo(int id)
    {
        IGlyphInfo? info = null;
        
        foreach (var provider in _providers)
        {
            var info2 = provider.GetGlyph(id);
            if (info2 == null) continue;
            info ??= info2;

            if (!HasFishyAdvance(info2))
                return new GlyphInfoFilter(info, info2);
        }

        if (info != null)
            return new GlyphInfoFilter(info, SpecialGlyphs.Missing);
        
        return GlyphInfoFilter.Missing;
    }

    private static bool HasFishyAdvance(IGlyphInfo info)
    {
        var f = info.GetAdvance(false);
        if (f is >= 0 or <= LargeForwardAdvance) return true;

        var g = info.GetAdvance(true);
        return g is < 0 or > LargeForwardAdvance;
    }
    
    public IGlyphInfo GetGlyphInfo(int i, bool filterFishyGlyphs) => 
        _glyphInfos.ComputeIfAbsent(i, ComputeGlyphInfo).Select(filterFishyGlyphs);

    private void CloseProviders()
    {
        foreach (var provider in _providers)
        {
            provider.Dispose();
        }
        
        _providers.Clear();
    }
    
    private void CloseTextures()
    {
        foreach (var texture in _textures)
        {
            texture.Dispose();
        }
        
        _textures.Clear();
    }
    
    public void Dispose()
    {
        CloseProviders();
        CloseTextures();
    }

    public BakedGlyph GetRandomGlyph(IGlyphInfo info)
    {
        var list = _glyphsByWidth.GetValueOrDefault((int) Math.Ceiling(info.GetAdvance(false)));
        return list != null && list.Any()
            ? GetGlyph(list[Random.Next(list.Count)])
            : _missingGlyph;
    }

    public BakedGlyph GetGlyph(int codepoint) => _glyphs.ComputeIfAbsent(codepoint, ComputeBakedGlyph);

    private BakedGlyph ComputeBakedGlyph(int id)
    {
        foreach (var provider in _providers)
        {
            var glyph = provider.GetGlyph(id);
            if (glyph != null)
                return glyph.Bake(Stitch);
        }

        return _missingGlyph;
    }

    private BakedGlyph Stitch(ISheetGlyphInfo info)
    {
        foreach (var texture in _textures)
        {
            var glyph = texture.Add(info);
            if (glyph != null) return glyph;
        }

        var location = _name.WithSuffix($"/{_textures.Count}");
        var bl = info.IsColored;
        var renderTypes = bl
            ? GlyphRenderTypes.CreateForColorTexture(location)
            : GlyphRenderTypes.CreateForIntensityTexture(location);
        
        var tex = new FontTexture(renderTypes, bl);
        tex.Texture!.Name = $"FontTexture - {location}";
        _textures.Add(tex);
        _textureManager.Register(location, tex);

        var g = tex.Add(info);
        return g ?? _missingGlyph;
    }

    private record GlyphInfoFilter(IGlyphInfo GlyphInfo, IGlyphInfo GlyphInfoNotFishy)
    {
        public static GlyphInfoFilter Missing { get; } = new(SpecialGlyphs.Missing, SpecialGlyphs.Missing);

        public IGlyphInfo Select(bool notFishy) => notFishy ? GlyphInfoNotFishy : GlyphInfo;
    }
}