namespace MiaCrate.Core;

public sealed class Direction
{
    private readonly int _data3d;
    private readonly int _oppositeIndex;
    private readonly int _data2d;
    private readonly string _name;
    private readonly AxisDirection _axisDirection;

    public Axis Axis { get; }
    public Vec3I Normal { get; }
    public int StepX => Normal.X;
    public int StepY => Normal.Y;
    public int StepZ => Normal.Z;

    private Direction(int data3d, int oppositeIndex, int data2d, String name, AxisDirection axisDirection, Axis axis, Vec3I normal)
    {
        _data3d = data3d;
        _oppositeIndex = oppositeIndex;
        _data2d = data2d;
        _name = name;
        _axisDirection = axisDirection;
        Axis = axis;
        Normal = normal;
    }
}