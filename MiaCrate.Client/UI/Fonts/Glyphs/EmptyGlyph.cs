using OpenTK.Mathematics;

namespace MiaCrate.Client.UI;

public class EmptyGlyph : BakedGlyph
{
    public static EmptyGlyph Instance { get; } = new();

    public EmptyGlyph() 
        : base(GlyphRenderTypes.CreateForColorTexture(new ResourceLocation("")), 
            0, 0, 0, 0, 0, 0, 0, 0)
    {
    }

    public override void Render(bool bl, float f, float g, Matrix4 matrix, IVertexConsumer consumer, float h, float i, float j, float k,
        int l)
    {
        
    }
}