using MiaCrate.Client.Extensions;
using MiaCrate.Client.Models.Json;
using MiaCrate.Core;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Models;

public class BlockElement
{
    private readonly Vector3 _from;
    private readonly Vector3 _to;
    private readonly Dictionary<Direction, BlockElementFace> _faces;
    private readonly BlockElementRotation? _rotation;
    private readonly bool _shade;

    public BlockElement(Vector3 from, Vector3 to, Dictionary<Direction, BlockElementFace> faces,
        BlockElementRotation? rotation, bool shade)
    {
        _from = from;
        _to = to;
        _faces = faces;
        _rotation = rotation;
        _shade = shade;
    }
    
    internal BlockElement(JsonBlockElement payload)
        : this(
            payload.FromPoint.ToVector3(),
            payload.ToPoint.ToVector3(),
            payload.Faces.ToDictionary(),
            FromPayload(payload.Rotation),
            payload.Shade)
    {
    }
    
    private static BlockElementRotation? FromPayload(JsonBlockElementRotation? rotation) => 
        rotation == null ? null : new BlockElementRotation(rotation);
}

public class BlockElementFace
{
    public const int NoTint = -1;
    
    private readonly Direction? _cullForDirection;
    private readonly int _tintIndex;
    private readonly string _texture;
    private readonly BlockFaceUv _uv;

    public BlockElementFace(Direction? cullForDirection, int tintIndex, string texture, BlockFaceUv uv)
    {
        _cullForDirection = cullForDirection;
        _tintIndex = tintIndex;
        _texture = texture;
        _uv = uv;
    }
    
    internal BlockElementFace(JsonBlockElementFace payload)
        : this(payload.CullFace, payload.TintIndex, payload.Texture, new BlockFaceUv(payload))
    {
        
    }
}