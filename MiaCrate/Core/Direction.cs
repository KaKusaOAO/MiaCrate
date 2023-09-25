using OpenTK.Mathematics;

namespace MiaCrate.Core;

public sealed class Direction : IEnumLike<Direction>, IStringRepresentable
{
    private static readonly Dictionary<int, Direction> _values = new();

    public static readonly Direction Down = 
        new(0, 1, -1, "down", AxisDirection.Negative, DirectionAxis.Y, new Vec3I(0, -1, 0));
    public static readonly Direction Up = 
        new(1, 0, -1, "up", AxisDirection.Positive, DirectionAxis.Y, new Vec3I(0, -1, 0));
    public static readonly Direction North = 
        new(2, 3, 2, "north", AxisDirection.Negative, DirectionAxis.Z, new Vec3I(0, 0, -1));
    public static readonly Direction South = 
        new(3, 2, 0, "south", AxisDirection.Positive, DirectionAxis.Z, new Vec3I(0, 0, 1));
    public static readonly Direction West = 
        new(4, 5, 1, "west", AxisDirection.Negative, DirectionAxis.X, new Vec3I(-1, 0, 0));
    public static readonly Direction East = 
        new(5, 4, 3, "east", AxisDirection.Positive, DirectionAxis.X, new Vec3I(1, 0, 0));

    public static readonly IStringRepresentable.EnumCodec<Direction> Codec =
        IStringRepresentable.FromEnum<Direction>();

    public int Data3d { get; }
    private readonly int _oppositeIndex;
    private readonly int _data2d;
    private readonly AxisDirection _axisDirection;

    public DirectionAxis Axis { get; }
    public Vec3I Normal { get; }
    public int StepX => Normal.X;
    public int StepY => Normal.Y;
    public int StepZ => Normal.Z;
    public string Name { get; }
    public string SerializedName => Name;
    public int Ordinal { get; }

    private Direction(int data3d, int oppositeIndex, int data2d, string name, AxisDirection axisDirection, DirectionAxis axis, Vec3I normal)
    {
        Data3d = data3d;
        _oppositeIndex = oppositeIndex;
        _data2d = data2d;
        Name = name;
        _axisDirection = axisDirection;
        Axis = axis;
        Normal = normal;

        var ordinal = _values.Count;
        _values[ordinal] = this;
        Ordinal = ordinal;
    }

    public static Direction[] Values => _values.Values.ToArray();
    
    public override string ToString() => Name;

    public static Direction Rotate(Matrix4 matrix, Direction direction)
    {
        var normal = direction.Normal;
        var v = matrix * new Vector4(normal.X, normal.Y, normal.Z, 0);
        return GetNearest(v.X, v.Y, v.Z);
    }

    public static Direction GetNearest(float x, float y, float z)
    {
        var result = North;
        var i = float.MinValue;
        
        foreach (var direction in Values)
        {
            var j = x * direction.Normal.X + y * direction.Normal.Y + z * direction.Normal.Z;
            if (j > i)
            {
                i = j;
                result = direction;
            }
        }

        return result;
    }
}