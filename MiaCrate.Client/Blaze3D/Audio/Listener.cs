using System.Numerics;
using MiaCrate.World.Phys;
using OpenTK.Audio.OpenAL;
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
        AL.Listener(ALListenerfv.Orientation, new []
        {
            a.X, a.Y, a.Z,
            b.X, b.Y, b.Z
        });
    }
    
    public void SetOrientation(TKVector3 a, TKVector3 b)
    {
        AL.Listener(ALListenerfv.Orientation, new []
        {
            a.X, a.Y, a.Z,
            b.X, b.Y, b.Z
        });
    }

    private void InternalSetPosition(Vec3 position)
    {
        _position = position;
        AL.Listener(ALListener3f.Position, 
            (float)position.X, (float)position.Y, (float)position.Z);
    }

    private void InternalSetGain(float gain)
    {
        AL.Listener(ALListenerf.Gain, gain);
        _gain = gain;
    }

    public void Reset()
    {
        Position = Vec3.Zero;
        Gain = 1;
    }
}