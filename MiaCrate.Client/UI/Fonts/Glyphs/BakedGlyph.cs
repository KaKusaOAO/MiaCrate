using MiaCrate.Client.Graphics;
using OpenTK.Mathematics;

namespace MiaCrate.Client.UI;

public class BakedGlyph
{
    private readonly GlyphRenderTypes _renderTypes;
    private readonly float _u0;
    private readonly float _u1;
    private readonly float _v0;
    private readonly float _v1;
    private readonly float _left;
    private readonly float _right;
    private readonly float _up;
    private readonly float _down;

    public BakedGlyph(GlyphRenderTypes renderTypes, float u0, float u1, float v0, float v1, float left, float right,
        float up, float down)
    {
        _renderTypes = renderTypes;
        _u0 = u0;
        _u1 = u1;
        _v0 = v0;
        _v1 = v1;
        _left = left;
        _right = right;
        _up = up;
        _down = down;
    }

    public virtual void Render(bool italic, float x, float y, Matrix4 matrix, IVertexConsumer consumer, float red, float green,
        float blue, float alpha, int l)
    {
        var left = x + _left;
        var right = x + _right;
        var up = _up - 3.0F;
        var down = _down - 3.0F;
        var y0 = y + up;
        var y1 = y + down;
        var x0 = italic ? 1.0F - 0.25F * up : 0.0F;
        var x1 = italic ? 1.0F - 0.25F * down : 0.0F;
        
        consumer.Vertex(matrix, left + x0, y0, 0.0F).Color(red, green, blue, alpha).Uv(_u0, _v0).Uv2(l).EndVertex();
        consumer.Vertex(matrix, left + x1, y1, 0.0F).Color(red, green, blue, alpha).Uv(_u0, _v1).Uv2(l).EndVertex();
        consumer.Vertex(matrix, right + x1, y1, 0.0F).Color(red, green, blue, alpha).Uv(_u1, _v1).Uv2(l).EndVertex();
        consumer.Vertex(matrix, right + x0, y0, 0.0F).Color(red, green, blue, alpha).Uv(_u1, _v0).Uv2(l).EndVertex();
    }

    public RenderType RenderType(Font.DisplayMode mode)
    {
        return _renderTypes.Select(mode);
    }
    
    public void RenderEffect(Effect effect, Matrix4 pose, IVertexConsumer consumer, int packedLightCoords)
    {
        consumer.Vertex(pose, effect.X0, effect.Y0, effect.Depth)
            .Color(effect.R, effect.G, effect.B, effect.A)
            .Uv(_u0, _v0).Uv2(packedLightCoords).EndVertex();
        
        consumer.Vertex(pose, effect.X1, effect.Y0, effect.Depth)
            .Color(effect.R, effect.G, effect.B, effect.A)
            .Uv(_u0, _v1).Uv2(packedLightCoords).EndVertex();
        
        consumer.Vertex(pose, effect.X1, effect.Y1, effect.Depth)
            .Color(effect.R, effect.G, effect.B, effect.A)
            .Uv(_u1, _v1).Uv2(packedLightCoords).EndVertex();
        
        consumer.Vertex(pose, effect.X0, effect.Y1, effect.Depth)
            .Color(effect.R, effect.G, effect.B, effect.A)
            .Uv(_u1, _v0).Uv2(packedLightCoords).EndVertex();
    }

    public record Effect(float X0, float Y0, float X1, float Y1, float Depth, float R, float G, float B, float A);

}