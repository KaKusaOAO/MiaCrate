using System.Diagnostics;
using System.Runtime.Intrinsics;
using MiaCrate.Client.Models;
using MiaCrate.Client.Resources;
using MiaCrate.Core;
using MiaCrate.Extensions;
using Mochi.Utils;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class FaceBakery
{
    public const int VertexIntSize = 8;
    public const int VertexCount = 4;
    private const int ColorIndex = 3;
    public const int UvIndex = 4;

    private static float Rescale22_5 { get; } = 1 / MathF.Cos(22.5f * (float) Mth.DegToRad) - 1;
    private static float Rescale45 { get; } = 1 / MathF.Cos(45f * (float) Mth.DegToRad) - 1;

    public BakedQuad BakeQuad(Vector3 from, Vector3 to, BlockElementFace face, TextureAtlasSprite sprite,
        Direction direction, IModelState modelState, BlockElementRotation? rotation, bool shade, ResourceLocation location)
    {
        var uv = face.Uv;
        if (modelState.IsUvLocked)
        {
            uv = RecomputeUvs(face.Uv, direction, modelState.Rotation, location);
        }

        var arr = new float[uv.Uvs!.Length];
        Array.Copy(uv.Uvs!, 0, arr, 0, arr.Length);

        var f = sprite.UvShrinkRatio;
        var g = (uv.Uvs[0] + uv.Uvs[0] + uv.Uvs[2] + uv.Uvs[2]) / 4;
        var h = (uv.Uvs[1] + uv.Uvs[1] + uv.Uvs[3] + uv.Uvs[3]) / 4;

        uv.Uvs[0] = Mth.Lerp(f, uv.Uvs[0], g);
        uv.Uvs[2] = Mth.Lerp(f, uv.Uvs[2], g);
        uv.Uvs[1] = Mth.Lerp(f, uv.Uvs[1], h);
        uv.Uvs[3] = Mth.Lerp(f, uv.Uvs[3], h);

        var iArr = MakeVertices(uv, sprite, direction, SetupShape(from, to), modelState.Rotation, rotation, shade);
        var d2 = CalculateFacing(iArr);
        Array.Copy(arr, 0, uv.Uvs, 0, arr.Length);

        if (rotation == null)
        {
            RecalculateWinding(iArr, d2);
        }

        return new BakedQuad(iArr, face.TintIndex, d2, sprite, shade);
    }

    private void RecalculateWinding(int[] arr, Direction direction)
    {
        var iArr = new int[arr.Length];
        Array.Copy(arr, 0, iArr, 0, arr.Length);

        var fs = new float[Direction.GetDirections().Length];
        fs[FaceInfo.Constants.MinX] = 999f;
        fs[FaceInfo.Constants.MinY] = 999f;
        fs[FaceInfo.Constants.MinZ] = 999f;
        fs[FaceInfo.Constants.MaxX] = -999f;
        fs[FaceInfo.Constants.MaxY] = -999f;
        fs[FaceInfo.Constants.MaxZ] = -999f;

        for (var i = 0; i < VertexCount; i++)
        {
            var j = VertexIntSize * i;

            var f = BitConverter.Int32BitsToSingle(iArr[j]);
            var g = BitConverter.Int32BitsToSingle(iArr[j + 1]);
            var h = BitConverter.Int32BitsToSingle(iArr[j + 2]);

            if (f < fs[FaceInfo.Constants.MinX])
                fs[FaceInfo.Constants.MinX] = f;
            if (g < fs[FaceInfo.Constants.MinY])
                fs[FaceInfo.Constants.MinY] = g;
            if (h < fs[FaceInfo.Constants.MinZ])
                fs[FaceInfo.Constants.MinZ] = h;
            
            if (f > fs[FaceInfo.Constants.MaxX])
                fs[FaceInfo.Constants.MaxX] = f;
            if (g > fs[FaceInfo.Constants.MaxY])
                fs[FaceInfo.Constants.MaxY] = g;
            if (h > fs[FaceInfo.Constants.MaxZ])
                fs[FaceInfo.Constants.MaxZ] = h;
        }

        var faceInfo = FaceInfo.FromFacing(direction);

        for (var j = 0; j < VertexCount; j++)
        {
            var k = VertexIntSize * j;
            var vertexInfo = faceInfo.GetVertexInfo(j);
            var h = fs[vertexInfo.XFace];
            var l = fs[vertexInfo.YFace];
            var m = fs[vertexInfo.ZFace];

            arr[k] = BitConverter.SingleToInt32Bits(h);
            arr[k + 1] = BitConverter.SingleToInt32Bits(l);
            arr[k + 2] = BitConverter.SingleToInt32Bits(m);

            for (var n = 0; n < VertexCount; n++)
            {
                var o = VertexIntSize * n;
                var p = BitConverter.Int32BitsToSingle(iArr[o]);
                var q = BitConverter.Int32BitsToSingle(iArr[o + 1]);
                var r = BitConverter.Int32BitsToSingle(iArr[o + 2]);

                const float tolerance = 1.0e-5f;
                if (Math.Abs(h - p) < tolerance && Math.Abs(l - q) < tolerance && Math.Abs(m - r) < tolerance)
                {
                    arr[k + VertexCount] = iArr[o + VertexCount];
                    arr[k + VertexCount + 1] = iArr[o + VertexCount + 1];
                }
            }
        }
    }

    private Direction CalculateFacing(int[] arr)
    {
        var v = new Vector3(
            BitConverter.Int32BitsToSingle(arr[0]), 
            BitConverter.Int32BitsToSingle(arr[1]),
            BitConverter.Int32BitsToSingle(arr[2]));
        var v2 = new Vector3(
            BitConverter.Int32BitsToSingle(arr[8]), 
            BitConverter.Int32BitsToSingle(arr[9]),
            BitConverter.Int32BitsToSingle(arr[10]));
        var v3 = new Vector3(
            BitConverter.Int32BitsToSingle(arr[16]), 
            BitConverter.Int32BitsToSingle(arr[17]),
            BitConverter.Int32BitsToSingle(arr[18]));

        var v4 = v - v2;
        var v5 = v3 - v2;
        var v6 = Vector3.Normalize(Vector3.Cross(v5, v4));

        if (!v6.IsFinite()) return Direction.Up;

        Direction? direction = null;
        var f = 0f;
        
        foreach (var dir in Direction.GetDirections())
        {
            var normal = dir.Normal;
            var v7 = new Vector3(normal.X, normal.Y, normal.Z);
            
            var g = Vector3.Dot(v6, v7);
            if (g >= 0 && g > f)
            {
                f = g;
                direction = dir;
            }
        }

        return direction ?? Direction.Up;
    }

    private float[] SetupShape(Vector3 from, Vector3 to)
    {
        var arr = new float[Direction.GetDirections().Length];
        arr[FaceInfo.Constants.MinX] = from.X / 16;
        arr[FaceInfo.Constants.MinY] = from.Y / 16;
        arr[FaceInfo.Constants.MinZ] = from.Z / 16;
        arr[FaceInfo.Constants.MaxX] = to.X / 16;
        arr[FaceInfo.Constants.MaxY] = to.Y / 16;
        arr[FaceInfo.Constants.MaxZ] = to.Z / 16;
        return arr;
    }

    private int[] MakeVertices(BlockFaceUv uv, TextureAtlasSprite sprite, Direction direction, float[] shape,
        Transformation transformation, BlockElementRotation? rotation, bool shade)
    {
        var arr = new int[VertexIntSize * VertexCount];

        for (var i = 0; i < VertexCount; i++)
        {
            BakeVertex(arr, i, direction, uv, shape, sprite, transformation, rotation, shade);   
        }

        return arr;
    }

    private void BakeVertex(int[] arr, int i, Direction direction, BlockFaceUv uv, float[] shape,
        TextureAtlasSprite sprite, Transformation transformation, BlockElementRotation? rotation, bool shade)
    {
        var vertexInfo = FaceInfo.FromFacing(direction).GetVertexInfo(i);
        var v = new Vector3(shape[vertexInfo.XFace], shape[vertexInfo.YFace], shape[vertexInfo.ZFace]);
        ApplyElementRotation(ref v, rotation);
        ApplyModelRotation(ref v, transformation);
        FillVertex(arr, i, v, sprite, uv);
    }

    private void FillVertex(int[] arr, int i, Vector3 v, TextureAtlasSprite sprite, BlockFaceUv uv)
    {
        var j = i * VertexIntSize;
        arr[j + 0] = BitConverter.SingleToInt32Bits(v.X);
        arr[j + 1] = BitConverter.SingleToInt32Bits(v.Y);
        arr[j + 2] = BitConverter.SingleToInt32Bits(v.Z);
        arr[j + 3] = -1;
        arr[j + 4] = BitConverter.SingleToInt32Bits(sprite.GetU(uv.GetU(i) / 16f));
        arr[j + 5] = BitConverter.SingleToInt32Bits(sprite.GetV(uv.GetV(i) / 16f));
    }

    private void ApplyModelRotation(ref Vector3 v, Transformation transformation)
    {
        if (transformation == Transformation.Identity) return;
        RotateVertexBy(ref v, Vector3.One / 2, transformation.Matrix, Vector3.One);
    }

    private void ApplyElementRotation(ref Vector3 v, BlockElementRotation? rotation)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (rotation == null) return;

        Vector3 v2;
        Vector3 v3;

        if (rotation.Axis == DirectionAxis.X)
        {
            v2 = Vector3.UnitX;
            v3 = Vector3.One - Vector3.UnitX;
        } else if (rotation.Axis == DirectionAxis.Y)
        {
            v2 = Vector3.UnitY;
            v3 = Vector3.One - Vector3.UnitY;
        } else if (rotation.Axis == DirectionAxis.Z)
        {
            v2 = Vector3.UnitZ;
            v3 = Vector3.One - Vector3.UnitZ;
        }
        else
        {
            throw new ArgumentException("There are only 3 axes");
        }

        var q = Quaternion.FromAxisAngle(v2, rotation.Angle * (float) Mth.DegToRad);
        if (rotation.Rescale)
        {
            if (MathF.Abs(rotation.Angle) == 22.5f)
            {
                v3 *= Rescale22_5;
            }
            else
            {
                v3 *= Rescale45;
            }
            
            v3 += Vector3.One;
        }
        else
        {
            v3 = Vector3.One;
        }

        RotateVertexBy(ref v, new Vector3(rotation.Origin), Matrix4.CreateFromQuaternion(q), v3);
    }

    private void RotateVertexBy(ref Vector3 v, Vector3 v2, Matrix4 matrix, Vector3 v3)
    {
        var v4 = matrix * new Vector4(v.X - v2.X, v.Y - v2.Y, v.Z - v2.Z, 1);
        v4 *= new Vector4(v3, 1);
        v = new Vector3(v4.X + v2.X, v4.Y + v2.Y, v4.Z + v2.Z);
    }

    public static BlockFaceUv RecomputeUvs(BlockFaceUv uv, Direction direction, Transformation transformation,
        ResourceLocation location)
    {
        throw new NotImplementedException();
    }
}