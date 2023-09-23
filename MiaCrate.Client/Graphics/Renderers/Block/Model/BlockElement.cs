using MiaCrate.Client.Extensions;
using MiaCrate.Client.Models;
using MiaCrate.Client.Models.Json;
using MiaCrate.Core;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class BlockElement
{
    public Vector3 From { get; }
    public Vector3 To { get; }
    public Dictionary<Direction, BlockElementFace> Faces { get; }
    public BlockElementRotation? Rotation { get; }
    public bool Shade { get; }

    public BlockElement(Vector3 from, Vector3 to, Dictionary<Direction, BlockElementFace> faces,
        BlockElementRotation? rotation, bool shade)
    {
        From = from;
        To = to;
        Faces = faces;
        Rotation = rotation;
        Shade = shade;
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