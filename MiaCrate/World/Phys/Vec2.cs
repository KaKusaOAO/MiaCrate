namespace MiaCrate.World.Phys;

public readonly struct Vec2
{
    public static Vec2 Zero { get; } = new();
    
    public float X { get; }
    public float Y { get; }

    public float Length => MathF.Sqrt(LengthSquared);
    public float LengthSquared => X * X + Y * Y;

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public static Vec2 operator +(Vec2 a, Vec2 b) => a.Add(b);
    public static Vec2 operator -(Vec2 a, Vec2 b) => a.Add(b.Negated());
    public static Vec2 operator -(Vec2 v) => v.Negated();
    
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public static bool operator ==(Vec2 a, Vec2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vec2 a, Vec2 b) => a.X != b.X || a.Y != b.Y;
    public static bool operator ==(Vec2? a, Vec2? b) => a.HasValue == b.HasValue && (!a.HasValue || a.Value == b!.Value);
    public static bool operator !=(Vec2? a, Vec2? b) => a.HasValue != b.HasValue || (a.HasValue && b.HasValue && a.Value == b.Value);

    public Vec2 Scale(float f) => new(X * f, Y * f);

    public float Dot(Vec2 other) => X * other.X + Y * other.Y;

    public Vec2 Add(Vec2 other) => new(X + other.X, Y + other.Y);
    public Vec2 Add(float n) => new(X + n, Y + n);

    public Vec2 Normalized()
    {
        var f = MathF.Sqrt(X * X + Y * Y);
        return f < 1e-4f ? Zero : new Vec2(X / f, Y / f);
    }

    public float DistanceToSqr(Vec2 other)
    {
        var dx = other.X - X;
        var dy = other.Y - Y;
        return dx * dx + dy * dy;
    }

    public Vec2 Negated() => new(-X, -Y);
    
    public override bool Equals(object? obj)
    {
        if (obj is not Vec2 v) return false;
        return X == v.X && Y == v.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}