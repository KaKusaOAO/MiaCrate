namespace MiaCrate;

public abstract class FloatProvider : ISampledFloat
{
    public abstract IFloatProviderType Type { get; }
    public abstract float MinValue { get; }
    public abstract float MaxValue { get; }
    public abstract float Sample(IRandomSource random);
}