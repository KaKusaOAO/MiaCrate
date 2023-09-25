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
}