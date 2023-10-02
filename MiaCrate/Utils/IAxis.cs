using Mochi.Utils;
using OpenTK.Mathematics;

namespace MiaCrate;

public interface IAxis
{
    // ReSharper disable InconsistentNaming
    public static IAxis XN { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitX, -f));
    public static IAxis XP { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitX, f));
    public static IAxis YN { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitY, -f));
    public static IAxis YP { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitY, f));
    public static IAxis ZN { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitZ, -f));
    public static IAxis ZP { get; } = Create(f => Quaternion.FromAxisAngle(Vector3.UnitZ, f));
    // ReSharper restore InconsistentNaming
    
    Quaternion Rotation(float radians);

    Quaternion RotationDegrees(float degrees) => Rotation(degrees * (float) Mth.DegToRad);

    private static IAxis Create(Func<float, Quaternion> func) => new Instance(func);

    private class Instance : IAxis
    {
        private readonly Func<float, Quaternion> _func;

        public Instance(Func<float, Quaternion> func)
        {
            _func = func;
        }


        public Quaternion Rotation(float radians) => _func(radians);
    }
}