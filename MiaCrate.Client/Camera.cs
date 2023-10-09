using MiaCrate.Core;
using MiaCrate.World;
using MiaCrate.World.Entities;
using MiaCrate.World.Phys;
using OpenTK.Mathematics;

namespace MiaCrate.Client;

public class Camera
{
    public const float FogDistanceScale = 0.083333336f;
    
    private bool _initialized;
    private IBlockGetter? _level;
    private Entity? _entity;
    private Vec3 _position = Vec3.Zero;

    private readonly MutableBlockPos _blockPosition = new();
    private Vector3 _forwards = Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _left = Vector3.UnitX;
    private Quaternion _rotation = Quaternion.Identity;

    private float _xRot;
    private float _yRot;
    private bool _detached;
    private float _eyeHeight;
    private float _eyeHeightOld;

    public void Setup(IBlockGetter level, Entity entity, bool detached, bool bl2, float f)
    {
        _initialized = true;
        _level = level;
        _entity = entity;
        _detached = detached;

        throw new NotImplementedException();
    }
}