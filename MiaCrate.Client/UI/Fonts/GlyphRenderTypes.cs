using MiaCrate.Client.Graphics;

namespace MiaCrate.Client.UI;

public record GlyphRenderTypes(RenderType Normal, RenderType SeeThrough, RenderType PolygonOffset)
{
    public static GlyphRenderTypes CreateForIntensityTexture(ResourceLocation location)
    {
        return new GlyphRenderTypes(
            RenderType.TextIntensity(location),
            RenderType.TextIntensitySeeThrough(location),
            RenderType.TextIntensityPolygonOffset(location)
        );
    }
    
    public static GlyphRenderTypes CreateForColorTexture(ResourceLocation location)
    {
        return new GlyphRenderTypes(
            RenderType.Text(location),
            RenderType.TextSeeThrough(location),
            RenderType.TextPolygonOffset(location)
        );
    }

    public RenderType Select(Font.DisplayMode mode)
    {
        return mode switch
        {
            Font.DisplayMode.Normal => Normal,
            Font.DisplayMode.SeeThrough => SeeThrough,
            Font.DisplayMode.PolygonOffset => PolygonOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}