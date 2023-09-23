using OpenTK.Mathematics;

namespace MiaCrate.Extensions;

public static class MathExtension
{
    public static bool IsFinite(this Matrix4 matrix) =>
        float.IsFinite(matrix.M11) && float.IsFinite(matrix.M12) && float.IsFinite(matrix.M13) && float.IsFinite(matrix.M14) && 
        float.IsFinite(matrix.M21) && float.IsFinite(matrix.M22) && float.IsFinite(matrix.M23) && float.IsFinite(matrix.M24) && 
        float.IsFinite(matrix.M31) && float.IsFinite(matrix.M32) && float.IsFinite(matrix.M33) && float.IsFinite(matrix.M34) && 
        float.IsFinite(matrix.M41) && float.IsFinite(matrix.M42) && float.IsFinite(matrix.M43) && float.IsFinite(matrix.M44);

    public static bool IsFinite(this Vector3 v) =>
        float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);

    private static Matrix3 Rotate(this Matrix3 matrix3, Quaternion q, ref Matrix3 destination)
    {
        var w2 = q.W * q.W;
        var x2 = q.X * q.X;
        var y2 = q.Y * q.Y;
        var z2 = q.Z * q.Z;

        var zw = q.Z * q.W;
        var dzw = zw + zw;
        var xy = q.X * q.Y;
        var dxy = xy + xy;

        var xz = q.X * q.Z;
        var dxz = xz + xz;
        var yw = q.Y * q.W;
        var dyw = yw + yw;

        var yz = q.Y * q.Z;
        var dyz = yz + yz;
        var xw = q.X * q.W;
        var dxw = xw + xw;

        var rm00 = w2 + x2 - z2 - y2;
        var rm01 = dxy + dzw;
        var rm02 = dxz - dyw;
        var rm10 = dxy - dzw;
        var rm11 = y2 - z2 + w2 - x2;
        var rm12 = dyz + dxw;
        var rm20 = dyw + dxz;
        var rm21 = dyz - dxw;
        var rm22 = z2 - y2 - x2 + w2;
        
        var nm00 = matrix3.M11 * rm00 + matrix3.M21 * rm01 + matrix3.M31 * rm02;
        var nm01 = matrix3.M12 * rm00 + matrix3.M22 * rm01 + matrix3.M32 * rm02;
        var nm02 = matrix3.M13 * rm00 + matrix3.M23 * rm01 + matrix3.M33 * rm02;
        var nm10 = matrix3.M11 * rm10 + matrix3.M21 * rm11 + matrix3.M31 * rm12;
        var nm11 = matrix3.M12 * rm10 + matrix3.M22 * rm11 + matrix3.M32 * rm12;
        var nm12 = matrix3.M13 * rm10 + matrix3.M23 * rm11 + matrix3.M33 * rm12;

        destination.M31 = matrix3.M11 * rm20 + matrix3.M21 * rm21 + matrix3.M31 * rm22;
        destination.M32 = matrix3.M12 * rm20 + matrix3.M22 * rm21 + matrix3.M32 * rm22;
        destination.M33 = matrix3.M13 * rm20 + matrix3.M23 * rm21 + matrix3.M33 * rm22;
        destination.M11 = nm00;
        destination.M12 = nm01;
        destination.M13 = nm02;
        destination.M21 = nm10;
        destination.M22 = nm11;
        destination.M23 = nm12;
        return destination;
    }
    
    public static Matrix3 Rotate(ref this Matrix3 matrix3, Quaternion q)
    {
        matrix3.Rotate(q, ref matrix3);
        return matrix3;
    }
}