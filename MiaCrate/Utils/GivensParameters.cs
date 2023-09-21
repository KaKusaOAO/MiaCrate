using OpenTK.Mathematics;

namespace MiaCrate;

public record GivensParameters(float SinHalf, float CosHalf)
{
    public float Cos => CosHalf * CosHalf - SinHalf * SinHalf;
    public float Sin => 2 * SinHalf * CosHalf;
    public GivensParameters Inverse => this with {SinHalf = -SinHalf};

    public static GivensParameters FromUnnormalized(float f, float g)
    {
        var h = 1 / MathF.Sqrt(f * f + g * g);
        return new GivensParameters(h * f, h * g);
    }
    
    public static GivensParameters FromPositiveAngle(float f)
    {
        var g = MathF.Sin(f / 2);
        var h = Util.CosFromSin(g, f / 2);
        return new GivensParameters(g, h);
    }

    public Quaternion AroundX(ref Quaternion quaternion)
    {
        quaternion.X = SinHalf;
        quaternion.Y = 0;
        quaternion.Z = 0;
        quaternion.W = CosHalf;
        return quaternion;
    }
    
    public Quaternion AroundY(ref Quaternion quaternion)
    {
        quaternion.X = 0;
        quaternion.Y = SinHalf;
        quaternion.Z = 0;
        quaternion.W = CosHalf;
        return quaternion;
    }
    
    public Quaternion AroundZ(ref Quaternion quaternion)
    {
        quaternion.X = 0;
        quaternion.Y = 0;
        quaternion.Z = SinHalf;
        quaternion.W = CosHalf;
        return quaternion;
    }

    public Matrix3 AroundX(ref Matrix3 matrix)
    {
        matrix.M12 = 0;
        matrix.M13 = 0;
        matrix.M21 = 0;
        matrix.M31 = 0;

        var c = Cos;
        var s = Sin;
        matrix.M22 = c;
        matrix.M33 = c;
        matrix.M23 = s;
        matrix.M32 = -s;
        matrix.M11 = 1;
        return matrix;
    } 
    
    public Matrix3 AroundY(ref Matrix3 matrix)
    {
        matrix.M12 = 0;
        matrix.M21 = 0;
        matrix.M23 = 0;
        matrix.M32 = 0;

        var c = Cos;
        var s = Sin;
        matrix.M11 = c;
        matrix.M33 = c;
        matrix.M13 = -s;
        matrix.M31 = s;
        matrix.M22 = 1;
        return matrix;
    } 
    
    public Matrix3 AroundZ(ref Matrix3 matrix)
    {
        matrix.M13 = 0;
        matrix.M23 = 0;
        matrix.M31 = 0;
        matrix.M32 = 0;

        var c = Cos;
        var s = Sin;
        matrix.M11 = c;
        matrix.M22 = c;
        matrix.M12 = s;
        matrix.M21 = -s;
        matrix.M33 = 1;
        return matrix;
    } 
}