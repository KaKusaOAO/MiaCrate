using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Core;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Resources;

public class BuiltInModel : IBakedModel
{
    public bool UsesBlockLight { get; }
    public TextureAtlasSprite ParticleIcon { get; }
    public ItemTransforms Transforms { get; }
    public ItemOverrides Overrides { get; }
    public bool UseAmbientOcclusion => false;
    public bool IsGui3D => true;
    public bool IsCustomRenderer => true;

    public BuiltInModel(ItemTransforms itemTransforms, ItemOverrides overrides, TextureAtlasSprite particleTexture,
        bool usesBlockLight)
    {
        Transforms = itemTransforms;
        Overrides = overrides;
        ParticleIcon = particleTexture;
        UsesBlockLight = usesBlockLight;
    }

    public List<BakedQuad> GetQuads(BlockState? state, Direction? direction, IRandomSource randomSource) => new();
}