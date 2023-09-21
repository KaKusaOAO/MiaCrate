using MiaCrate.Extensions;
using OpenTK.Mathematics;

namespace MiaCrate;

public static class MatrixUtil
{
    private const float Threshold = 1.0e-6f;
    private static float G { get; } = 3 + 2 * MathF.Sqrt(2);
    private static GivensParameters Pi4 { get; } = GivensParameters.FromPositiveAngle(float.Pi / 4);

    public static void SvdDecompose(Matrix3 matrix3,
        out Quaternion leftRotation, out Vector3 scale, out Quaternion rightRotation) =>
        SvdDecompose(ref matrix3, out leftRotation, out scale, out rightRotation);
    
    public static void SvdDecompose(ref Matrix3 matrix3, 
        out Quaternion leftRotation, out Vector3 scale, out Quaternion rightRotation)
    {
        var m2 = Matrix3.Transpose(matrix3);
        m2 *= matrix3;

        var q = EigenvalueJacobi(ref m2, 5);
        var f = m2.M11;
        var g = m2.M22;
        var bl = f < Threshold;
        var bl2 = g < Threshold;

        var m4 = matrix3.Rotate(q);
        var q2 = Quaternion.Identity;
        var q3 = Quaternion.Identity;
        
        var givensParameters = bl 
            ? QrGivensQuat(m4.M22, -m4.M21) 
            : QrGivensQuat(m4.M11, m4.M12);
        
        var q4 = givensParameters.AroundZ(ref q3);
        var m5 = givensParameters.AroundZ(ref m2);
        q2 *= q4;
        m5 = Matrix3.Transpose(m5) * m4;

        givensParameters = (bl
            ? QrGivensQuat(m5.M33, -m5.M31)
            : QrGivensQuat(m5.M11, m5.M13)).Inverse;

        var q5 = givensParameters.AroundY(ref q3);
        var m6 = givensParameters.AroundY(ref m4);
        q2 *= q5;
        m6 = Matrix3.Transpose(m6) * m5;
        
        givensParameters = bl2 
            ? QrGivensQuat(m6.M33, -m6.M32) 
            : QrGivensQuat(m6.M22, m6.M23);

        var q6 = givensParameters.AroundX(ref q3);
        var m7 = givensParameters.AroundX(ref m5);
        q2 *= q6;
        m7 = Matrix3.Transpose(m7) * m6;
        
        // Assign results.
        leftRotation = q2;
        scale = new Vector3(m7.M11, m7.M22, m7.M33);
        rightRotation = Quaternion.Conjugate(q);
    }

    private static GivensParameters QrGivensQuat(float x, float y)
    {
        var h = (float) Math.Sqrt((double)x * x + (double)y * y);
        var i = h > Threshold ? y : 0;
        var j = MathF.Abs(x) + MathF.Max(h, Threshold);
        if (x < 0)
        {
            (i, j) = (j, i);
        }
        
        return GivensParameters.FromUnnormalized(i, j);
    }

    public static Quaternion EigenvalueJacobi(ref Matrix3 matrix, int i)
    {
        var q = Quaternion.Identity;
        var m = Matrix3.Identity;
        var q2 = Quaternion.Identity;

        for (var j = 0; j < i; j++)
        {
            StepJacobi(ref matrix, ref m, ref q2, ref q);
        }

        var result = Quaternion.Normalize(q);
        return result;
    }

    private static void StepJacobi(ref Matrix3 matrix3, ref Matrix3 matrix32, ref Quaternion quaternion, ref Quaternion quaternion2)
    {
        GivensParameters givensParameters;
        Quaternion q3;

        if (matrix3.M12 * matrix3.M12 + matrix3.M21 * matrix3.M21 > Threshold)
        {
            givensParameters = ApproxGivensQuat(matrix3.M11, 0.5f * (matrix3.M12 + matrix3.M21), matrix3.M22);
            q3 = givensParameters.AroundZ(ref quaternion);
            quaternion2 *= q3;
            givensParameters.AroundZ(ref matrix32);
            SimilarityTransform(ref matrix3, ref matrix32);
        }

        if (matrix3.M13 * matrix3.M13 + matrix3.M31 * matrix3.M31 > Threshold)
        {
            givensParameters = ApproxGivensQuat(matrix3.M11, 0.5f * (matrix3.M13 + matrix3.M31), matrix3.M33).Inverse;
            q3 = givensParameters.AroundY(ref quaternion);
            quaternion2 *= q3;
            givensParameters.AroundY(ref matrix32);
            SimilarityTransform(ref matrix3, ref matrix32);
        }

        if (matrix3.M23 * matrix3.M23 + matrix3.M32 * matrix3.M32 > Threshold)
        {
            givensParameters = ApproxGivensQuat(matrix3.M11, 0.5f * (matrix3.M23 + matrix3.M32), matrix3.M33);
            q3 = givensParameters.AroundX(ref quaternion);
            quaternion2 *= q3;
            givensParameters.AroundX(ref matrix32);
            SimilarityTransform(ref matrix3, ref matrix32);
        }
    }

    private static void SimilarityTransform(ref Matrix3 a, ref Matrix3 b)
    {
        a *= b;
        b = Matrix3.Transpose(b);
        b *= a;
        a = b;
    }

    private static GivensParameters ApproxGivensQuat(float f, float g, float h)
    {
        var i = 2 * (f - h);
        return G * g * g < i * i ? GivensParameters.FromUnnormalized(g, i) : Pi4;
    }
}