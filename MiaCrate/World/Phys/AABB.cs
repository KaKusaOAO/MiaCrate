using MiaCrate.Core;
using Mochi.Utils;

namespace MiaCrate.World.Phys;

public readonly struct AABB
{
    private const double Epsilon = 1e-7;

    // ReSharper disable InconsistentNaming
    public readonly double MinX;
    public readonly double MinY;
    public readonly double MinZ;
    public readonly double MaxX;
    public readonly double MaxY;
    public readonly double MaxZ;
    // ReSharper restore InconsistentNaming

    public double XSize => MaxX - MinX;
    public double YSize => MaxY - MinY;
    public double ZSize => MaxZ - MinZ;
    public double Size => (XSize + YSize + ZSize) / 3;

    public AABB(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        MinX = Math.Min(x1, x2);
        MinY = Math.Min(y1, y2);
        MinZ = Math.Min(z1, z2);
        MaxX = Math.Max(x1, x2);
        MaxY = Math.Max(y1, y2);
        MaxZ = Math.Max(z1, z2);
    }
    
    public AABB(BlockPos pos)
        : this(pos.X, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1) {}
    
    public AABB(BlockPos a, BlockPos b)
        : this(a.X, a.Y, a.Z, b.X, b.Y, b.Z) {}
    
    public AABB(Vec3 a, Vec3 b)
        : this(a.X, a.Y, a.Z, b.X, b.Y, b.Z) {}

    public AABB SetMinX(double d) => new(d, MinY, MinZ, MaxX, MaxY, MaxZ);
    public AABB SetMinY(double d) => new(MinX, d, MinZ, MaxX, MaxY, MaxZ);
    public AABB SetMinZ(double d) => new(MinX, MinY, d, MaxX, MaxY, MaxZ);
    public AABB SetMaxX(double d) => new(MinX, MinY, MinZ, d, MaxY, MaxZ);
    public AABB SetMaxY(double d) => new(MinX, MinY, MinZ, MaxX, d, MaxZ);
    public AABB SetMaxZ(double d) => new(MinX, MinY, MinZ, MaxX, MaxY, d);

    public double Min(DirectionAxis axis) => axis.Choose(MinX, MinY, MinZ);
    public double Max(DirectionAxis axis) => axis.Choose(MaxX, MaxY, MaxZ);

    public AABB Contract(double x, double y, double z)
    {
        var g = MinX;
        var h = MinY;
        var i = MinZ;
        var j = MaxX;
        var k = MaxY;
        var l = MaxZ;

        switch (x)
        {
            case < 0:
                g -= x;
                break;
            case > 0:
                j -= x;
                break;
        }

        switch (y)
        {
            case < 0:
                h -= y;
                break;
            case > 0:
                k -= y;
                break;
        }

        switch (z)
        {
            case < 0:
                i -= z;
                break;
            case > 0:
                l -= z;
                break;
        }

        return new AABB(g, h, i, j, k, l);
    }

    public AABB ExpandTowards(Vec3 v) => ExpandTowards(v.X, v.Y, v.Z);
    
    public AABB ExpandTowards(double x, double y, double z)
    {
        var g = MinX;
        var h = MinY;
        var i = MinZ;
        var j = MaxX;
        var k = MaxY;
        var l = MaxZ;

        switch (x)
        {
            case < 0:
                g += x;
                break;
            case > 0:
                j += x;
                break;
        }

        switch (y)
        {
            case < 0:
                h += y;
                break;
            case > 0:
                k += y;
                break;
        }

        switch (z)
        {
            case < 0:
                i += z;
                break;
            case > 0:
                l += z;
                break;
        }

        return new AABB(g, h, i, j, k, l);
    }

    public AABB Inflate(double x, double y, double z)
    {
        var g = MinX - x;
        var h = MinY - y;
        var i = MinZ - z;
        var j = MaxX + x;
        var k = MaxY + y;
        var l = MaxZ + z;
        
        return new AABB(g, h, i, j, k, l);
    }

    public AABB Inflate(double value) => Inflate(value, value, value);

    public AABB Intersect(AABB aabb)
    {
        var g = Math.Max(MinX, aabb.MinX);
        var h = Math.Max(MinY, aabb.MinY);
        var i = Math.Max(MinZ, aabb.MinZ);
        var j = Math.Min(MaxX, aabb.MaxX);
        var k = Math.Min(MaxY, aabb.MaxY);
        var l = Math.Min(MaxZ, aabb.MaxZ);
        
        return new AABB(g, h, i, j, k, l);
    }
    
    public AABB MinMax(AABB aabb)
    {
        var g = Math.Min(MinX, aabb.MinX);
        var h = Math.Min(MinY, aabb.MinY);
        var i = Math.Min(MinZ, aabb.MinZ);
        var j = Math.Max(MaxX, aabb.MaxX);
        var k = Math.Max(MaxY, aabb.MaxY);
        var l = Math.Max(MaxZ, aabb.MaxZ);
        
        return new AABB(g, h, i, j, k, l);
    }

    public AABB Move(double x, double y, double z) =>
        new(MinX + x, MinY + y, MinZ + z, MaxX + x, MaxY + y, MaxZ + z);
    
    
    public AABB Move(BlockPos pos) =>
        new(MinX + pos.X, MinY + pos.Y, MinZ + pos.Z, MaxX + pos.X, MaxY + pos.Y, MaxZ + pos.Z);

    public AABB Move(Vec3 pos) =>
        new(MinX + pos.X, MinY + pos.Y, MinZ + pos.Z, MaxX + pos.X, MaxY + pos.Y, MaxZ + pos.Z);

    public bool Intersects(double minX, double minY, double minZ, double maxX, double maxY, double maxZ) =>
        MinX < maxX && MaxX > minX && MinY < maxY && MaxY > minY && MinZ < maxZ && MaxZ > minZ;

    public bool Intersects(AABB aabb) =>
        Intersects(aabb.MinX, aabb.MinY, aabb.MinZ, aabb.MaxX, aabb.MaxY, aabb.MaxZ);

    public bool Intersects(Vec3 a, Vec3 b) =>
        Intersects(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z), 
            Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

    public bool Contains(double x, double y, double z) =>
        x >= MinX && x < MaxX && y >= MinY && y < MaxY && z >= MinZ && z < MaxZ;

    public bool Contains(Vec3 v) => Contains(v.X, v.Y, v.Z);

    public AABB Deflate(double x, double y, double z) => Inflate(-x, -y, -z);

    public AABB Deflate(double value) => Inflate(-value);

    public IOptional<Vec3> Clip(Vec3 a, Vec3 b)
    {
        var ds = 1.0;
        var d = b.X - a.X;
        var e = b.Y - a.Y;
        var f = b.Z - a.Z;

        var direction = GetDirection(this, a, ref ds, null, d, e, f);
        return direction == null 
            ? Optional.Empty<Vec3>() 
            : Optional.Of(a.Add(ds * d, ds * e, ds * f));
    }

    private static Direction? GetDirection(AABB aabb, Vec3 vec3, ref double ds, Direction? direction, double d,
        double e, double f)
    {
        if (d > Epsilon)
            direction = ClipPoint(ref ds, direction, d, e, f, aabb.MinX, aabb.MinY, aabb.MaxY, aabb.MinZ, aabb.MaxZ, Direction.West, vec3.X, vec3.Y, vec3.Z);
        else if (d < -Epsilon)
            direction = ClipPoint(ref ds, direction, d, e, f, aabb.MaxX, aabb.MinY, aabb.MaxY, aabb.MinZ, aabb.MaxZ, Direction.East, vec3.X, vec3.Y, vec3.Z);
        
        if (e > Epsilon)
            direction = ClipPoint(ref ds, direction, e, f, d, aabb.MinY, aabb.MinZ, aabb.MaxZ, aabb.MinX, aabb.MaxX, Direction.Down, vec3.Y, vec3.Z, vec3.X);
        else if (e < -Epsilon)
            direction = ClipPoint(ref ds, direction, e, f, d, aabb.MaxY, aabb.MinZ, aabb.MaxZ, aabb.MinX, aabb.MaxX, Direction.Up, vec3.Y, vec3.Z, vec3.X);
        
        if (f > Epsilon)
            direction = ClipPoint(ref ds, direction, f, d, e, aabb.MinZ, aabb.MinX, aabb.MaxX, aabb.MinY, aabb.MaxY, Direction.North, vec3.Z, vec3.X, vec3.Y);
        else if (f < -Epsilon)
            direction = ClipPoint(ref ds, direction, f, d, e, aabb.MaxZ, aabb.MinX, aabb.MaxX, aabb.MinY, aabb.MaxY, Direction.South, vec3.Z, vec3.X, vec3.Y);

        return direction;
    }

    private static Direction? ClipPoint(ref double ds, Direction? direction, double d, double e, double f, double g,
        double h, double i, double j, double k, Direction d2, double l, double m, double n)
    {
        var o = (g - l) / d;
        var p = m + o * e;
        var q = n + o * f;

        if (0 < o && o < ds &&
            h - Epsilon < p && p < i + Epsilon &&
            j - Epsilon < q && q < k + Epsilon)
        {
            ds = o;
            return d2;
        }

        return direction;
    }
}