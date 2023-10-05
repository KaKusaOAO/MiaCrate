namespace MiaCrate.Core;

public class Vec3I : IComparable<Vec3I>, IVec3I<Vec3I>
{
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public int Z { get; protected set; }

    public Vec3I(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vec3I Create(int x, int y, int z) => new(x, y, z);

    public int CompareTo(Vec3I? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var xComparison = X.CompareTo(other.X);
        if (xComparison != 0) return xComparison;
        var yComparison = Y.CompareTo(other.Y);
        if (yComparison != 0) return yComparison;
        return Z.CompareTo(other.Z);
    }
}

public interface IVec3I<T> where T : IVec3I<T>
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    
    public static abstract T Create(int x, int y, int z);
}

public static class Vec3IExtension
{
    public static T Offset<T>(this T v, int x, int y, int z) where T : IVec3I<T> => 
        x == 0 && y == 0 && z == 0 ? v : T.Create(v.X + x, v.Y + y, v.Z + z);

    public static T Relative<T>(this T v, Direction direction, int steps = 1) where T : IVec3I<T> =>
        steps == 0 
            ? v
            : T.Create(v.X + direction.StepX * steps, v.Y + direction.StepY * steps, v.Z + direction.StepZ * steps);
    
    public static T Below<T>(this T v, int steps = 1) where T : IVec3I<T> => 
        v.Relative(Direction.Down, steps);
} 