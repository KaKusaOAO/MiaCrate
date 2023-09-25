using System.Text.Json;
using MiaCrate.Client.Models;
using MiaCrate.Client.Models.Json;
using MiaCrate.Client.Resources;
using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.World.Items;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class BlockModel : IUnbakedModel
{
    private const char ReferenceChar = '#';
    public const string ParticleTextureReference = "particle";
    private const bool DefaultAmbientOcclusion = true;

    private static readonly FaceBakery _faceBakery = new();
    
    private readonly List<BlockElement> _elements;
    private readonly Dictionary<string, IEither<Material, string>> _textureMap;
    private readonly bool? _hasAmbientOcclusion;
    private readonly GuiLightType? _guiLight;
    private readonly ItemTransforms _transforms;
    public string Name { get; set; } = "";

    protected BlockModel? Parent { get; set; }
    
    protected ResourceLocation? ParentLocation { get; set; }
    public List<BlockElement> Elements => !_elements.Any() && Parent != null ? Parent.Elements : _elements;
    public bool HasAmbientOcclusion => _hasAmbientOcclusion ?? Parent?.HasAmbientOcclusion ?? DefaultAmbientOcclusion;
    public GuiLightType GuiLight => _guiLight ?? Parent?.GuiLight ?? GuiLightType.Side;
    public bool IsResolved => ParentLocation != null || Parent is { IsResolved: true };
    public List<ItemOverride> Overrides { get; }
    public BlockModel RootModel => Parent?.RootModel ?? this;

    public IEnumerable<ResourceLocation> Dependencies => throw new NotImplementedException();

    public BlockModel(ResourceLocation? parentLocation, List<BlockElement> elements,
        Dictionary<string, IEither<Material, string>> textureMap, bool? hasAmbientOcclusion, GuiLightType? guiLight,
        ItemTransforms transforms, List<ItemOverride> overrides)
    {
        ParentLocation = parentLocation;
        _elements = elements;
        _textureMap = textureMap;
        _hasAmbientOcclusion = hasAmbientOcclusion;
        _guiLight = guiLight;
        _transforms = transforms;
        Overrides = overrides;
    }

    private IEither<Material, string> FindTextureEntry(string name)
    {
        var model = this;
        while (model != null)
        {
            var either = model._textureMap.GetValueOrDefault(name);
            if (either != null) return either;
            model = model.Parent;
        }

        return Either
            .CreateLeft(new Material(TextureAtlas.LocationBlocks, MissingTextureAtlasSprite.Location))
            .Right<string>();
    }
    
    public Material GetMaterial(string str)
    {
        if (IsTextureReference(str)) str = str[1..];

        var list = new List<string>();
        while (true)
        {
            var either = FindTextureEntry(str);
            var optional = either.Left;
            if (optional.IsPresent) return optional.Value;

            str = either.Right.Value;
            if (list.Contains(str))
            {
                Logger.Warn("Unable to resolve texture due to reference chain " +
                            $"{string.Join("->", list)}->{str} in {Name}");
                return new Material(TextureAtlas.LocationBlocks, MissingTextureAtlasSprite.Location);
            }

            list.Add(str);
        }
    }

    private static bool IsTextureReference(string str) => str[0] == ReferenceChar;

    public override string ToString() => Name;

    public static BlockModel FromJson(JsonBlockModel payload)
    {
        var parent = string.IsNullOrEmpty(payload.Parent) ? null : new ResourceLocation(payload.Parent);
        var list = payload.Elements.Select(e => new BlockElement(e)).ToList();

        var location = TextureAtlas.LocationBlocks;
        var textureMap = payload.Textures.ToDictionary(
            e => e.Key,
            e => ParseTextureLocationOrReference(location, e.Value));
        
        return new BlockModel(parent, list, textureMap,
            payload.AmbientOcclusion, 
            GuiLightType.GetByName(payload.GuiLight), 
            new ItemTransforms(payload.Display), 
            new List<ItemOverride>());
    }

    private static IEither<Material, string> ParseTextureLocationOrReference(ResourceLocation location, string str)
    {
        if (IsTextureReference(str))
            return Either.CreateRight(str[1..]).Left<Material>();

        if (!ResourceLocation.TryParse(str, out var l))
            throw new Exception($"{str} is not a valid resource location");

        return Either.CreateLeft(new Material(location, l)).Right<string>();
    }
    
    public static BlockModel FromStream(Stream stream)
    {
        var json = JsonSerializer.Deserialize(stream, JsonBlockModelContext.Default.JsonBlockModel)!;
        return FromJson(json);
    }

    public static BlockModel FromString(string str)
    {
        var json = JsonSerializer.Deserialize(str, JsonBlockModelContext.Default.JsonBlockModel)!;
        return FromJson(json);
    }

    public void ResolveParents(Func<ResourceLocation, IUnbakedModel> func)
    {
        throw new NotImplementedException();
    }

    public ItemTransforms Transforms
    {
        get
        {
            var a = GetTransform(ItemDisplayContext.ThirdPersonLeftHand);
            var b = GetTransform(ItemDisplayContext.ThirdPersonRightHand);
            var c = GetTransform(ItemDisplayContext.FirstPersonLeftHand);
            var d = GetTransform(ItemDisplayContext.FirstPersonRightHand);
            var e = GetTransform(ItemDisplayContext.Head);
            var f = GetTransform(ItemDisplayContext.Gui);
            var g = GetTransform(ItemDisplayContext.Ground);
            var h = GetTransform(ItemDisplayContext.Fixed);
            return new ItemTransforms(a, b, c, d, e, f, g, h);
        }
    }

    private ItemTransform GetTransform(ItemDisplayContext context)
    {
        return Parent != null && _transforms.HasTransform(context)
            ? Parent.GetTransform(context)
            : _transforms.GetTransform(context);
    }

    private ItemOverrides GetItemOverrides(IModelBaker modelBaker, BlockModel model)
    {
        return !Overrides.Any()
            ? ItemOverrides.Empty
            : new ItemOverrides(modelBaker, model, Overrides);
    }

    public IBakedModel? Bake(IModelBaker modelBaker, Func<Material, TextureAtlasSprite> func, IModelState modelState,
        ResourceLocation location) =>
        Bake(modelBaker, this, func, modelState, location, true);
    
    public IBakedModel? Bake(IModelBaker modelBaker, BlockModel blockModel, 
        Func<Material, TextureAtlasSprite> func, IModelState modelState, ResourceLocation location, bool bl)
    {
        var sprite = func(GetMaterial(ParticleTextureReference));
        if (RootModel == ModelBakery.BlockEntityMarker)
            return new BuiltInModel(Transforms, GetItemOverrides(modelBaker, blockModel), sprite, GuiLight.LightLikeBlock);

        var builder = new SimpleBakedModel.Builder(this, GetItemOverrides(modelBaker, blockModel), bl)
            .Particle(sprite);

        foreach (var element in Elements)
        {
            foreach (var (direction, face) in element.Faces)
            {
                var sp = func(GetMaterial(face.Texture));
                var bakedFace = BakeFace(element, face, sp, direction, modelState, location);
                
                if (face.CullForDirection == null)
                {
                    builder.AddUnculledFace(bakedFace);
                }
                else
                {
                    builder.AddCulledFace(Direction.Rotate(modelState.Rotation.Matrix, face.CullForDirection), 
                        bakedFace);
                }
            }
        }

        return builder.Build();
    }

    private static BakedQuad BakeFace(BlockElement blockElement, BlockElementFace face, TextureAtlasSprite sprite,
        Direction direction, IModelState modelState, ResourceLocation location) =>
        _faceBakery.BakeQuad(blockElement.From, blockElement.To, face, sprite, direction, modelState,
            blockElement.Rotation, blockElement.Shade, location);

    public sealed class GuiLightType : IEnumLike<GuiLightType>
    {
        private static readonly Dictionary<int, GuiLightType> _values = new();

        public static GuiLightType Front { get; } = new("front");
        public static GuiLightType Side { get; } = new("side");
        
        private readonly string _name;

        public int Ordinal { get; }

        public static GuiLightType[] Values => _values.Values.ToArray();

        private GuiLightType(string name)
        {
            _name = name;

            Ordinal = _values.Count;
            _values[Ordinal] = this;
        }

        public static GuiLightType? GetByName(string? name)
        {
            if (name == null) return null;
            
            foreach (var type in _values.Values.Where(t => t._name == name))
            {
                return type;
            }

            throw new ArgumentException($"Invalid gui light: {name}");
        }

        public bool LightLikeBlock => this == Side;
    }
}