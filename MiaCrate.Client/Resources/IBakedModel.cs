using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Core;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Resources;

public interface IBakedModel
{
    public bool UseAmbientOcclusion { get; }
    public bool IsGui3D { get; }
    public bool UsesBlockLight { get; }
    public bool IsCustomRenderer { get; }
    public TextureAtlasSprite ParticleIcon { get; }
    public ItemTransforms Transforms { get; }
    public ItemOverrides Overrides { get; }

    public List<BakedQuad> GetQuads(BlockState? state, Direction? direction, IRandomSource randomSource);
}