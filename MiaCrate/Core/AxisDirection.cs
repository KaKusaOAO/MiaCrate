namespace MiaCrate.Core;

public sealed class AxisDirection
{
    public static readonly AxisDirection Positive = new(1, "Towards positive");
    public static readonly AxisDirection Negative = new(-1, "Towards negative");
    
    public int Step { get; }
    public string Name { get; }

    public AxisDirection Opposite => this == Positive ? Negative : Positive;
    
    private AxisDirection(int step, string name)
    {
        Step = step;
        Name = name;
    }
}