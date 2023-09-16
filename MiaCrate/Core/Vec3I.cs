namespace MiaCrate.Core;

public class Vec3I : IComparable<Vec3I>
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

public static class Vec3IExtension
{
    public static Vec3I Offset(this Vec3I v, int x, int y, int z) => 
        x == 0 && y == 0 && z == 0 ? v : new Vec3I(v.X + x, v.Y + y, v.Z + z);

    public static BlockPos Offset(this BlockPos v, int x, int y, int z) => 
        x == 0 && y == 0 && z == 0 ? v : new BlockPos(v.X + x, v.Y + y, v.Z + z);
} 