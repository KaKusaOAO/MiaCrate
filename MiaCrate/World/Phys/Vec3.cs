using System.Numerics;
using MiaCrate.Core;

namespace MiaCrate.World.Phys;

public class Vec3 : IPosition
{
    public static readonly Vec3 Zero = new(0, 0, 0); 
    
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3(Vector3 vector3)
    {
        X = vector3.X;
        Y = vector3.Y;
        Z = vector3.Z;
    }

    public static Vec3 operator +(Vec3 a, Vec3 b) => a.Add(b);
    public static Vec3 operator -(Vec3 a, Vec3 b) => a.Subtract(b);
    public static Vec3 operator *(Vec3 a, Vec3 b) => a.Multiply(b);
    public static Vec3 operator *(Vec3 v, double s) => v.Scale(s);
    public static implicit operator Vec3(Vector3 source) => new(source);
    public static implicit operator Vector3(Vec3 source) => source.ToVector3();

    public Vec3 Add(Vec3 other) => Add(other.X, other.Y, other.Z);
    public Vec3 Add(double x, double y, double z) => new(X + x, Y + y, Z + z);
    public Vec3 Subtract(Vec3 other) => Subtract(other.X, other.Y, other.Z);
    public Vec3 Subtract(double x, double y, double z) => new(X - x, Y - y, Z - z);
    public Vec3 Scale(double scale) => new(X * scale, Y * scale, Z * scale);
    public Vec3 Multiply(Vec3 other) => Multiply(other.X, other.Y, other.Z);
    public Vec3 Multiply(double x, double y, double z) => new(X * x, Y * y, Z * z);

    public double DistanceTo(Vec3 other) => DistanceTo(other.X, other.Y, other.Z);
    public double DistanceTo(double x, double y, double z) => (new Vec3(x, y, z) - this).Length;
    public double DistanceToSquared(Vec3 other) => DistanceToSquared(other.X, other.Y, other.Z);
    public double DistanceToSquared(double x, double y, double z) => (new Vec3(x, y, z) - this).LengthSquared;

    public Vec3 OffsetRandom(IRandomSource randomSource, float scale) => Add(
        (randomSource.NextSingle() - 0.5f) * scale,
        (randomSource.NextSingle() - 0.5f) * scale,
        (randomSource.NextSingle() - 0.5f) * scale
    );

    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => Math.Sqrt(LengthSquared);

    public Vector3 ToVector3() => new((float)X, (float)Y, (float)Z);

    public override string ToString() => $"({X}, {Y}, {Z})";
}