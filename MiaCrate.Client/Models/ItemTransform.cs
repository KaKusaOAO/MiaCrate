using MiaCrate.Client.Extensions;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Models;

public class ItemTransform
{
    public const float MaxTranslation = 5;
    public const float MaxScale = 4;
    
    private static readonly Vector3 _defaultRotation = Vector3.Zero;
    private static readonly Vector3 _defaultTranslation = Vector3.Zero;
    private static readonly Vector3 _defaultScale = Vector3.One;
    public static readonly ItemTransform NoTransform = new(_defaultRotation, _defaultTranslation, _defaultScale);
    
    private readonly Vector3 _rotation;
    private readonly Vector3 _translation;
    private readonly Vector3 _scale;

    public ItemTransform(Vector3 rotation, Vector3 translation, Vector3 scale)
    {
        _rotation = rotation;
        _translation = translation;
        _scale = scale;
    }

    internal ItemTransform(JsonItemTransform payload)
        : this(
            payload.Rotation?.ToVector3() ?? _defaultRotation,
            ClampTranslation(payload.Translation?.ToVector3() ?? _defaultTranslation),
            ClampScale(payload.Scale?.ToVector3() ?? _defaultScale))
    {
        
    }

    private static Vector3 ClampTranslation(Vector3 v)
    {
        v /= 16;
        v.X = Math.Clamp(v.X, -MaxTranslation, MaxTranslation);
        v.Y = Math.Clamp(v.Y, -MaxTranslation, MaxTranslation);
        v.Z = Math.Clamp(v.Z, -MaxTranslation, MaxTranslation);
        return v;
    }
    
    private static Vector3 ClampScale(Vector3 v)
    {
        v.X = Math.Clamp(v.X, -MaxScale, MaxScale);
        v.Y = Math.Clamp(v.Y, -MaxScale, MaxScale);
        v.Z = Math.Clamp(v.Z, -MaxScale, MaxScale);
        return v;
    }
}