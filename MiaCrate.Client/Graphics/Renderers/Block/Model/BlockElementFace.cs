using MiaCrate.Client.Models;
using MiaCrate.Client.Models.Json;
using MiaCrate.Core;

namespace MiaCrate.Client.Graphics;

public class BlockElementFace
{
    public const int NoTint = -1;

    public Direction? CullForDirection { get; }
    public int TintIndex { get; }
    public string Texture { get; }
    public BlockFaceUv Uv { get; }

    public BlockElementFace(Direction? cullForDirection, int tintIndex, string texture, BlockFaceUv uv)
    {
        CullForDirection = cullForDirection;
        TintIndex = tintIndex;
        Texture = texture;
        Uv = uv;
    }
    
    internal BlockElementFace(JsonBlockElementFace payload)
        : this(payload.CullFace, payload.TintIndex, payload.Texture, new BlockFaceUv(payload))
    {
        
    }
}