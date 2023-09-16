using MiaCrate.Client.Graphics;
using Mochi.Structs;

namespace MiaCrate.Client;

public interface IBufferVertexConsumer : IVertexConsumer
{
    VertexFormatElement CurrentElement { get; }
    void NextElement();
    void PutByte(int i, byte b);
    void PutShort(int i, short s);
    void PutFloat(int i, float f);

    IVertexConsumer IVertexConsumer.Vertex(double x, double y, double z) =>
        DefaultVertex(this, x, y, z);

    protected static IVertexConsumer DefaultVertex(IBufferVertexConsumer consumer, double x, double y, double z)
    {
        if (consumer.CurrentElement.Usage != VertexFormatElement.UsageInfo.Position) return consumer;
        if (consumer.CurrentElement.Type != VertexFormatElement.TypeInfo.Float || consumer.CurrentElement.Count != 3)
            throw new InvalidOperationException();

        consumer.PutFloat(0, (float) x);
        consumer.PutFloat(4, (float) y);
        consumer.PutFloat(8, (float) z);
        consumer.NextElement();
        return consumer;
    }

    public new IVertexConsumer Color(int red, int green, int blue, int alpha) =>
        DefaultColorImpl(this, red, green, blue, alpha);

    IVertexConsumer IVertexConsumer.Color(int red, int green, int blue, int alpha) =>
        Color(red, green, blue, alpha);

    protected static IVertexConsumer DefaultColorImpl(IBufferVertexConsumer consumer, int red, int green, int blue,
        int alpha)
    {
        if (consumer.CurrentElement.Usage != VertexFormatElement.UsageInfo.Color) return consumer;
        if (consumer.CurrentElement.Type != VertexFormatElement.TypeInfo.UByte || consumer.CurrentElement.Count != 4)
            throw new InvalidOperationException();

        consumer.PutByte(0, (byte) red);
        consumer.PutByte(1, (byte) green);
        consumer.PutByte(2, (byte) blue);
        consumer.PutByte(3, (byte) alpha);
        consumer.NextElement();
        return consumer;
    }

    IVertexConsumer IVertexConsumer.Uv(float u, float v) => DefaultUv(this, u, v);

    protected static IVertexConsumer DefaultUv(IBufferVertexConsumer consumer, float u, float v)
    {
        if (consumer.CurrentElement.Usage != VertexFormatElement.UsageInfo.Uv || consumer.CurrentElement.Index != 0) 
            return consumer;
        if (consumer.CurrentElement.Type != VertexFormatElement.TypeInfo.Float || consumer.CurrentElement.Count != 2)
            throw new InvalidOperationException();

        consumer.PutFloat(0, u);
        consumer.PutFloat(0, v);
        consumer.NextElement();
        return consumer;
    }

    IVertexConsumer UvShort(short s, short t, int index)
    {
        if (CurrentElement.Usage != VertexFormatElement.UsageInfo.Uv || CurrentElement.Index != index) return this;
        if (CurrentElement.Type != VertexFormatElement.TypeInfo.Short || CurrentElement.Count != 2)
            throw new InvalidOperationException();

        PutFloat(0, s);
        PutFloat(2, t);
        NextElement();
        return this;
    }

    IVertexConsumer IVertexConsumer.OverlayCoords(int i, int j) => DefaultOverlayCoords(this, i, j);
    IVertexConsumer IVertexConsumer.Uv2(int i, int j) => DefaultUv2(this, i, j);

    protected static IVertexConsumer DefaultOverlayCoords(IBufferVertexConsumer consumer, int i, int j) =>
        consumer.UvShort((short)i, (short)j, 1);
    
    protected static IVertexConsumer DefaultUv2(IBufferVertexConsumer consumer, int i, int j) =>
        consumer.UvShort((short)i, (short)j, 2);

    IVertexConsumer IVertexConsumer.Normal(float x, float y, float z) =>
        DefaultNormal(this, x, y, z);

    protected static IVertexConsumer DefaultNormal(IBufferVertexConsumer consumer, float x, float y, float z)
    {
        if (consumer.CurrentElement.Usage != VertexFormatElement.UsageInfo.Normal) return consumer;
        if (consumer.CurrentElement.Type != VertexFormatElement.TypeInfo.Byte || consumer.CurrentElement.Count != 3)
            throw new InvalidOperationException();
        
        consumer.PutByte(0, NormalIntValue(x));
        consumer.PutByte(1, NormalIntValue(y));
        consumer.PutByte(2, NormalIntValue(z));
        consumer.NextElement();
        return consumer;
    }

    protected static byte NormalIntValue(float f) => (byte) ((int) (Math.Clamp(f, -1f, 1f) * 127) & 0xff);
}