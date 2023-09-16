namespace MiaCrate.Client;

public interface IVertexConsumer
{
    IVertexConsumer Vertex(double x, double y, double z);
    IVertexConsumer Color(int red, int green, int blue, int alpha);
    IVertexConsumer Uv(float u, float v);
    IVertexConsumer OverlayCoords(int i, int j);
    IVertexConsumer Uv2(int i, int j);
    IVertexConsumer Normal(float x, float y, float z);
    void EndVertex();

    void DefaultColor(int red, int green, int blue, int alpha);
    void UnsetDefaultColor();

    void Vertex(float x, float y, float z, float red, float green, float blue, float alpha, float u, float v, 
        int oPacked, int uv2Packed, float nx, float ny, float nz) => 
        DefaultVertex(this, x, y, z, red, green, blue, alpha, u, v, oPacked, uv2Packed, nx, ny, nz);
    
    protected static void DefaultVertex(IVertexConsumer consumer, 
        float x, float y, float z, float red, float green, float blue, float alpha, float u, float v, 
        int oPacked, int uv2Packed, float nx, float ny, float nz) => consumer.Vertex(x, y, z)
        .Color(red, green, blue, alpha)
        .Uv(u, v)
        .OverlayCoords(oPacked)
        .Uv2(uv2Packed)
        .Normal(nx, ny, nz)
        .EndVertex();
}

public static class VertexConsumerExtension
{
    public static IVertexConsumer OverlayCoords(this IVertexConsumer consumer, int packed) => 
        consumer.OverlayCoords(packed & 0xffff, (packed >> 16) & 0xffff);
    
    public static IVertexConsumer Uv2(this IVertexConsumer consumer, int packed) => 
        consumer.Uv2(packed & 0xffff, (packed >> 16) & 0xffff);

    public static IVertexConsumer Color(this IVertexConsumer consumer, float red, float green, float blue,
        float alpha) =>
        consumer.Color((int)(red * 255), (int)(green * 255), (int)(blue * 255), (int)(alpha * 255));
}