using System.Text.Json;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models.Json;
using MiaCrate.Data;
using Mochi.Utils;

namespace MiaCrate.Client.Models;

public class BlockModel : IUnbakedModel
{
    private const char ReferenceChar = '#';
    public const string ParticleTextureReference = "particle";
    private const bool DefaultAmbientOcclusion = true;
    
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
        var json = JsonSerializer.Deserialize<JsonBlockModel>(stream)!;
        return FromJson(json);
    }

    public sealed class GuiLightType : IEnumLike<GuiLightType>
    {
        private static readonly Dictionary<int, GuiLightType> _values = new();

        public static readonly GuiLightType Front = new("front");
        public static readonly GuiLightType Side = new("side");
        
        private readonly string _name;

        public int Ordinal { get; }

        private GuiLightType(string name)
        {
            _name = name;

            Ordinal = _values.Count;
            _values[Ordinal] = this;
        }

        public static GuiLightType GetByName(string name)
        {
            foreach (var type in _values.Values.Where(t => t._name == name))
            {
                return type;
            }

            throw new ArgumentException($"Invalid gui light: {name}");
        }

        public bool LightLikeBlock => this == Side;
    }
}