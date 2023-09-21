using MiaCrate.Extensions;
using OpenTK.Mathematics;
namespace MiaCrate;

public sealed class Transformation
{
    private readonly Matrix4 _matrix;

    public static Transformation Identity { get; } = new(Matrix4.Identity)
    {
        _translation = Vector3.Zero,
        _leftRotation = Quaternion.Identity,
        _scale = Vector3.One,
        _rightRotation = Quaternion.Identity,
        _decomposed = true
    };
    
    private bool _decomposed;
    private Vector3 _translation;
    private Quaternion _leftRotation;
    private Vector3 _scale;
    private Quaternion _rightRotation;

    public Matrix4 Matrix => _matrix;

    public Vector3 Translation
    {
        get
        {
            EnsureDecomposed();
            return _translation;
        }
    }

    public Quaternion LeftRotation
    {
        get
        {
            EnsureDecomposed();
            return _leftRotation;
        }
    }

    public Vector3 Scale
    {
        get
        {
            EnsureDecomposed();
            return _scale;
        }
    }

    public Quaternion RightRotation
    {
        get
        {
            EnsureDecomposed();
            return _rightRotation;
        }
    }

    public Transformation(Matrix4? matrix)
    {
        _matrix = matrix ?? Matrix4.Identity;
    }

    public Transformation? Inverse()
    {
        if (this == Identity) return this;
        var invert = Matrix4.Invert(_matrix);
        return invert.IsFinite() ? new Transformation(invert) : null;
    }

    private void EnsureDecomposed()
    {
        if (_decomposed) return;

        var f = 1 / _matrix.M44;
        var m = _matrix * f;
        MatrixUtil.SvdDecompose(new Matrix3(m), out _leftRotation, out _scale, out _rightRotation);
        
        _translation = _matrix.Row3.Xyz * f;
        _decomposed = true;
    }

    private static Matrix4 Compose(Vector3? translation, Quaternion? leftRotation, Vector3? scale,
        Quaternion? rightRotation)
    {
        var result = Matrix4.Identity;
        
        if (translation.HasValue) 
            result *= Matrix4.CreateTranslation(translation.Value);

        if (leftRotation.HasValue)
            result *= Matrix4.CreateFromQuaternion(leftRotation.Value);

        if (scale.HasValue)
            result *= Matrix4.CreateScale(scale.Value);

        if (rightRotation.HasValue)
            result *= Matrix4.CreateFromQuaternion(rightRotation.Value);

        return result;
    }
    
}