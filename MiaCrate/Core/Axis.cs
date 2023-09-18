namespace MiaCrate.Core;

public sealed class Axis : IEnumLike<Axis>, IStringRepresentable
{
    private static readonly Dictionary<int, Axis> _values = new();

    public static readonly Axis X = new("x", AxisComponent.X);
    public static readonly Axis Y = new("y", AxisComponent.Y);
    public static readonly Axis Z = new("z", AxisComponent.Z);

    public static readonly IStringRepresentable.EnumCodec<Axis> Codec = IStringRepresentable.FromEnum(GetAxes);
    private readonly AxisComponent _component;
    
    public int Ordinal { get; }
    public string Name { get; }
    public string SerializedName => Name;
    public bool IsVertical => this == Y;
    public bool IsHorizontal => this == X || this == Z;

    private Axis(string name, AxisComponent component)
    {
        _component = component;
        Name = name;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values[ordinal] = this;
    }

    public static Axis[] GetAxes() => _values.Values.ToArray();

    public bool Test(Direction? direction) => direction != null && direction.Axis == this;

    public int Choose(int x, int y, int z)
    {
        return _component switch
        {
            AxisComponent.X => x,
            AxisComponent.Y => y,
            AxisComponent.Z => z,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public double Choose(double x, double y, double z)
    {
        return _component switch
        {
            AxisComponent.X => x,
            AxisComponent.Y => y,
            AxisComponent.Z => z,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private enum AxisComponent
    {
        X, Y, Z
    }
}