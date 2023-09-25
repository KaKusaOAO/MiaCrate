using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Core;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Resources;

public class SimpleBakedModel : IBakedModel
{
    private readonly List<BakedQuad> _unculledFaces;
    private readonly Dictionary<Direction, List<BakedQuad>> _culledFaces;
    
    public bool UseAmbientOcclusion { get; }
    public bool UsesBlockLight { get; }
    public bool IsCustomRenderer => false;
    public bool IsGui3D { get; }
    public TextureAtlasSprite ParticleIcon { get; }
    public ItemTransforms Transforms { get; }
    public ItemOverrides Overrides { get; }

    public SimpleBakedModel(List<BakedQuad> unculledFaces, Dictionary<Direction, List<BakedQuad>> culledFaces, bool hasAmbientOcclusion, bool usesBlockLight, bool isGui3D, TextureAtlasSprite particleIcon, ItemTransforms transforms, ItemOverrides overrides)
    {
        _unculledFaces = unculledFaces;
        _culledFaces = culledFaces;
        UseAmbientOcclusion = hasAmbientOcclusion;
        UsesBlockLight = usesBlockLight;
        IsGui3D = isGui3D;
        ParticleIcon = particleIcon;
        Transforms = transforms;
        Overrides = overrides;
    }

    public List<BakedQuad> GetQuads(BlockState? state, Direction? direction, IRandomSource randomSource) => 
        direction == null ? _unculledFaces : _culledFaces[direction];

    public class Builder
    {
        private readonly List<BakedQuad> _unculledFaces = new();
        private readonly Dictionary<Direction, List<BakedQuad>> _culledFaces = new();
        private readonly bool _hasAmbientOcclusion;
        private readonly bool _usesBlockLight;
        private readonly bool _isGui3D;
        private readonly ItemTransforms _transforms;
        private readonly ItemOverrides _overrides;
        private TextureAtlasSprite? _particleIcon;
        
        public Builder(BlockModel blockModel, ItemOverrides overrides, bool bl)
            : this(blockModel.HasAmbientOcclusion, blockModel.GuiLight.LightLikeBlock, bl, 
                blockModel.Transforms, overrides) {}

        public Builder(bool hasAmbientOcclusion, bool usesBlockLight, bool isGui3D, ItemTransforms transforms, ItemOverrides overrides)
        {
            foreach (var direction in Direction.Values)
            {
                _culledFaces.Add(direction, new List<BakedQuad>());   
            }

            _hasAmbientOcclusion = hasAmbientOcclusion;
            _usesBlockLight = usesBlockLight;
            _isGui3D = isGui3D;
            _transforms = transforms;
            _overrides = overrides;
        }
        
        public Builder AddCulledFace(Direction direction, BakedQuad quad)
        {
            _culledFaces[direction].Add(quad);
            return this;
        }

        public Builder AddUnculledFace(BakedQuad quad)
        {
            _unculledFaces.Add(quad);
            return this;
        }

        public Builder Particle(TextureAtlasSprite sprite)
        {
            _particleIcon = sprite;
            return this;
        }

        public IBakedModel Build()
        {
            if (_particleIcon == null)
                throw new Exception("Missing particle!");

            return new SimpleBakedModel(_unculledFaces, _culledFaces, _hasAmbientOcclusion, _usesBlockLight, _isGui3D,
                _particleIcon, _transforms, _overrides);
        }
    }
}