using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class AbstractUniform
{
    public virtual void Set(float f1) {}
    public virtual void Set(float f1, float f2) {}
    public virtual void Set(float f1, float f2, float f3) {}
    public virtual void Set(float f1, float f2, float f3, float f4) {}
    public virtual void SetSafe(float f1, float f2, float f3, float f4) {}
    public virtual void SetSafe(int i1, int i2, int i3, int i4) {}
    public virtual void Set(int i1) {}
    public virtual void Set(int i1, int i2) {}
    public virtual void Set(int i1, int i2, int i3) {}
    public virtual void Set(int i1, int i2, int i3, int i4) {}
    public virtual void Set(float[] fs) {}
    public virtual void Set(Vector3 v3) {}
    public virtual void Set(Vector4 v4) {}
    public virtual void SetMat2X2(float f1, float f2, float f3, float f4) {}
    public virtual void SetMat2X3(float f1, float f2, float f3, float f4, float f5, float f6) {}
    public virtual void SetMat2X4(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8) {}
    public virtual void SetMat3X2(float f1, float f2, float f3, float f4, float f5, float f6) {}
    public virtual void SetMat3X3(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8, float f9) {}
    public virtual void SetMat3X4(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8, float f9, float f10, float f11, float f12) {}
    public virtual void SetMat4X2(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8) {}
    public virtual void SetMat4X3(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8, float f9, float f10, float f11, float f12) {}
    public virtual void SetMat4X4(float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8, float f9, float f10, float f11, float f12, float f13, float f14, float f15, float f16) {}
    public virtual void Set(Matrix4 matrix) {}
    public virtual void Set(Matrix3 matrix) {}
}