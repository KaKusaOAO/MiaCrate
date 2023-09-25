using MiaCrate.Data;

namespace MiaCrate.Core;

public sealed class DirectionAxis : IEnumLike<DirectionAxis>, IStringRepresentable
{
    private static readonly Dictionary<int, DirectionAxis> _values = new();

    public static readonly DirectionAxis X = new("x", AxisComponent.X);
    public static readonly DirectionAxis Y = new("y", AxisComponent.Y);
    public static readonly DirectionAxis Z = new("z", AxisComponent.Z);

    public static IStringRepresentable.EnumCodec<DirectionAxis> Codec { get; } =
        IStringRepresentable.FromEnum<DirectionAxis>();
    
    private readonly AxisComponent _component;
    
    public int Ordinal { get; }
    public string Name { get; }
    public string SerializedName => Name;
    public bool IsVertical => this == Y;
    public bool IsHorizontal => this == X || this == Z;

    private DirectionAxis(string name, AxisComponent component)
    {
        _component = component;
        Name = name;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values[ordinal] = this;
    }

    public static DirectionAxis[] Values => _values.Values.ToArray();

    public static DirectionAxis? ByName(string name) => Codec.ByName(name);

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