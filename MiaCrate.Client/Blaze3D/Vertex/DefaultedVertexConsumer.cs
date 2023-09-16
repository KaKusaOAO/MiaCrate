namespace MiaCrate.Client;

public abstract class DefaultedVertexConsumer : IVertexConsumer
{
    protected bool IsDefaultColorSet { get; private set; }
    protected int DefaultR { get; private set;}
    protected int DefaultG { get; private set;}
    protected int DefaultB { get; private set;}
    protected int DefaultA { get; private set;}
    
    public virtual void DefaultColor(int red, int green, int blue, int alpha)
    {
        DefaultR = red;
        DefaultG = green;
        DefaultB = blue;
        DefaultA = alpha;
        IsDefaultColorSet = true;
    }

    public virtual void UnsetDefaultColor()
    {
        IsDefaultColorSet = false;
    }
    
    public abstract IVertexConsumer Vertex(double x, double y, double z);
    public abstract IVertexConsumer Color(int red, int green, int blue, int alpha);
    public abstract IVertexConsumer Uv(float u, float v);
    public abstract IVertexConsumer OverlayCoords(int i, int j);
    public abstract IVertexConsumer Uv2(int i, int j);
    public abstract IVertexConsumer Normal(float x, float y, float z);
    public abstract void EndVertex();
}