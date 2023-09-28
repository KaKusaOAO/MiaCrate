using System.Numerics;
using MiaCrate.World.Phys;
using TKVector3 = OpenTK.Mathematics.Vector3;

namespace MiaCrate.Client.Audio;

public class Listener
{
    private float _gain;
    private Vec3 _position = Vec3.Zero;

    public float Gain
    {
        get => _gain;
        set => InternalSetGain(value);
    }

    public Vec3 Position
    {
        get => _position;
        set => InternalSetPosition(value);
    }

    public void SetOrientation(Vector3 a, Vector3 b)
    {
        
    }
    
    public void SetOrientation(TKVector3 a, TKVector3 b)
    {
        
    }

    private void InternalSetPosition(Vec3 position)
    {
        _position = position;
        
    }

    private void InternalSetGain(float gain)
    {
        
    }

    public void Reset()
    {
        Position = Vec3.Zero;
        Gain = 1;
    }
}