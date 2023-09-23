using MiaCrate.Client.Models;
using MiaCrate.Core;

namespace MiaCrate.Client.Graphics;

public record BakedQuad(int[] Vertices, int TintIndex, Direction Direction, TextureAtlasSprite Sprite, bool Shade)
{
    public bool IsTinted => TintIndex != BlockElementFace.NoTint;
}