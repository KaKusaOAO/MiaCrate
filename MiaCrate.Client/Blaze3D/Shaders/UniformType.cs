namespace MiaCrate.Client.Shaders;

public enum UniformType
{
    Unknown = -1,
    Int,
    Int2,
    Int3,
    Int4,
    Float,
    Float2,
    Float3,
    Float4,
    Matrix2,
    Matrix3,
    Matrix4
}

public static class UniformTypeExtension
{
    public static bool IsInt(this UniformType type) => type <= UniformType.Int4 && type != UniformType.Unknown;
    public static bool IsFloat(this UniformType type) => type is >= UniformType.Float and <= UniformType.Float4;
    public static bool IsMatrix(this UniformType type) => type is >= UniformType.Matrix2 and <= UniformType.Matrix4;
    public static bool IsFloatBuffer(this UniformType type) => type.IsFloat() || type.IsMatrix();
}