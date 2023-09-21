using System.Text;
using System.Text.Json.Nodes;
using MiaCrate.Client.Colors;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World.Blocks;
using Mochi.Utils;

namespace MiaCrate.Client.Resources;

public class ModelBakery
{
    public const int DestroyStageCount = 10;
    private const int InvisibleModelGroup = 0;
    private const string BuiltinSlash = "builtin/";
    private const string BuiltinSlashGenerated = $"{BuiltinSlash}generated";
    private const string BuiltinBlockEntity = $"{BuiltinSlash}entity";
    private const string MissingModelName = "missing";
    
    public static string MissingModelMesh { get; } = Util.Make(() =>
    {
        // TODO: Rewrite with JSON objects
        var sb = new StringBuilder();
        
        sb.AppendLine(@$"{{");
        sb.AppendLine(@$"    ""textures"": {{");
        sb.AppendLine(@$"        ""particle"": ""{MissingTextureAtlasSprite.Location.Path}"",");
        sb.AppendLine(@$"        ""missingno"": ""{MissingTextureAtlasSprite.Location.Path}""");
        sb.AppendLine(@$"    }},");
        sb.AppendLine(@$"    ""elements"": [");
        sb.AppendLine(@$"        {{");
        sb.AppendLine(@$"            ""from"": [0, 0, 0],");
        sb.AppendLine(@$"            ""to"": [16, 16, 16],");
        sb.AppendLine(@$"            ""faces"": {{");
        sb.AppendLine(@$"                ""down"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""down"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }},");
        sb.AppendLine(@$"                ""up"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""up"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }},");
        sb.AppendLine(@$"                ""north"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""north"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }},");
        sb.AppendLine(@$"                ""south"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""south"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }},");
        sb.AppendLine(@$"                ""west"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""west"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }},");
        sb.AppendLine(@$"                ""east"": {{");
        sb.AppendLine(@$"                    ""uv"": [0, 0, 16, 16],");
        sb.AppendLine(@$"                    ""cullface"": ""east"",");
        sb.AppendLine(@$"                    ""texture"": ""#missingno""");
        sb.AppendLine(@$"                }}");
        sb.AppendLine(@$"            }}");
        sb.AppendLine(@$"        }}");
        sb.AppendLine(@$"    ]");
        sb.AppendLine(@$"}}");

        return sb.ToString();
    });

    private static readonly Dictionary<string, string> _builtinModels = new()
    {
        ["missing"] = MissingModelMesh
    };
        
    public static ModelResourceLocation MissingModelLocation { get; } = 
        ModelResourceLocation.Vanilla($"{BuiltinSlash}missing", "missing");
    
    public static FileToIdConverter BlockStateLister { get; } = FileToIdConverter.Json("blockstates");
    public static FileToIdConverter ModelLister { get; } = FileToIdConverter.Json("models");

    private static readonly IStateDefinition<Block, BlockState> _itemFrameFakeDefinition =
        new StateDefinition<Block, BlockState>.Builder(Block.Air)
            .Add(BoolProperty.Create("map"))
            .Create(b => b.DefaultBlockState, BlockState.CreateFactory());

    private static readonly Dictionary<ResourceLocation, IStateDefinition<Block, BlockState>> _staticDefinitions =
        new()
        {
            [new ResourceLocation("item_frame")] = _itemFrameFakeDefinition,
            [new ResourceLocation("glow_item_frame")] = _itemFrameFakeDefinition
        };

    private readonly BlockModelDefinition.Context _context = new();
    private readonly HashSet<ResourceLocation> _loadingStack = new();
    private readonly Dictionary<ResourceLocation, IUnbakedModel> _unbakedCache = new();
    private readonly Dictionary<ResourceLocation, IUnbakedModel> _topLevelModels = new();
    
    private readonly BlockColors _blockColors;
    private readonly Dictionary<ResourceLocation, BlockModel> _modelResources;
    private readonly Dictionary<ResourceLocation, List<LoadedJson>> _blockStateResources;

    public static BlockModel GenerationMarker { get; } = Util.Make(BlockModel.FromString(@"{""gui_light"":""front""}"),
        m =>
        {
            m.Name = "generation marker";
        });
    
    public static BlockModel BlockEntityMarker { get; } = Util.Make(BlockModel.FromString(@"{""gui_light"":""side""}"),
        m =>
        {
            m.Name = "block entity marker";
        });

    public Dictionary<ResourceLocation, IBakedModel> BakedTopLevelModels { get; } = new();

    public ModelBakery(BlockColors blockColors, IProfilerFiller profiler,
        Dictionary<ResourceLocation, BlockModel> modelResources,
        Dictionary<ResourceLocation, List<LoadedJson>> blockStateResources)
    {
        _blockColors = blockColors;
        _modelResources = modelResources;
        _blockStateResources = blockStateResources;
        profiler.Push("missing_model");

        try
        {
            _unbakedCache.Add(MissingModelLocation, LoadBlockModel(MissingModelLocation));
            LoadTopLevel(MissingModelLocation);
        }
        catch (Exception ex)
        {
            Logger.Error("Error loading missing model, should never happen :(");
            Logger.Error(ex);
            throw;
        }
    }

    private void LoadTopLevel(ModelResourceLocation location)
    {
        var model = GetModel(location);
        _unbakedCache[location] = model;
        _topLevelModels[location] = model;
    }

    private BlockModel LoadBlockModel(ResourceLocation location)
    {
        var path = location.Path;
        if (BuiltinSlashGenerated == path) return GenerationMarker;
        if (BuiltinBlockEntity == path) return BlockEntityMarker;

        if (path.StartsWith(BuiltinSlash))
        {
            var subPath = path[BuiltinSlash.Length..];
            var str3 = _builtinModels.GetValueOrDefault(subPath);

            if (str3 == null)
                throw new FileNotFoundException(location.ToString());

            var model = BlockModel.FromString(str3);
            model.Name = location.ToString();
            return model;
        }

        var file = ModelLister.IdToFile(location);
        var m = _modelResources.GetValueOrDefault(file);
        if (m == null)
            throw new FileNotFoundException(file.ToString());

        m.Name = location.ToString();
        return m;
    }

    public void BakeModels(Func<ResourceLocation, Material, TextureAtlasSprite> func)
    {
        foreach (var resourceLocation in _topLevelModels.Keys)
        {
            IBakedModel? bakedModel = null;

            try
            {
                bakedModel =
                    new ModelBakerImpl(this, func, resourceLocation).Bake(resourceLocation, BlockModelRotation.X0_Y0);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to bake model: '{resourceLocation}': {ex}");
            }

            if (bakedModel != null)
            {
                BakedTopLevelModels[resourceLocation] = bakedModel;
            }
        }
    }

    public IUnbakedModel GetModel(ResourceLocation location)
    {
        if (_unbakedCache.TryGetValue(location, out var model))
            return model;

        if (_loadingStack.Contains(location))
            throw new InvalidOperationException($"Circular reference while loading {location}");

        _loadingStack.Add(location);

        var missing = _unbakedCache[MissingModelLocation];

        while (_loadingStack.Any())
        {
            var l = _loadingStack.First();

            try
            {
                if (!_unbakedCache.ContainsKey(l))
                {
                    LoadModel(l);
                }
            }
            catch (BlockStateDefinitionException ex)
            {
                Logger.Warn(ex.Message);
                _unbakedCache[l] = missing;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to load model: '{l}' referenced from: {location}: {ex}");
                _unbakedCache[l] = missing;
            }
            finally
            {
                _loadingStack.Remove(l);
            }
        }

        return _unbakedCache.GetValueOrDefault(location, missing);
    }

    private void LoadModel(ResourceLocation location)
    {
        if (location is not ModelResourceLocation modelLocation)
        {
            CacheAndQueueDependencies(location, LoadBlockModel(location));
            return;
        }

        ResourceLocation l;
        if (modelLocation.Variant == "inventory")
        {
            l = location.WithPrefix("item/");
            var model = LoadBlockModel(location);
            CacheAndQueueDependencies(location, model);
            _unbakedCache[l] = model;
        }
        else
        {
            l = new ResourceLocation(location.Namespace, location.Path);
            var stateDefinition =
                Optional.OfNullable(_staticDefinitions.GetValueOrDefault(l))
                    .OrElseGet(() => BuiltinRegistries.Block.Get(l)!.StateDefinition);
            _context.Definition = stateDefinition;
        }
        
        throw new NotImplementedException();
    }

    private void CacheAndQueueDependencies(ResourceLocation location, IUnbakedModel model)
    {
        _unbakedCache[location] = model;
        
        foreach (var resourceLocation in model.Dependencies)
        {
            _loadingStack.Add(resourceLocation);
        }
    }

    private class BlockStateDefinitionException : Exception
    {
        public BlockStateDefinitionException(string message) : base(message) {}
    }
    
    public record LoadedJson(string Source, JsonNode Data);

    private class ModelBakerImpl : IModelBaker
    {
        private readonly ModelBakery _instance;
        private readonly Func<Material, TextureAtlasSprite> _modelTextureGetter;

        public ModelBakerImpl(ModelBakery instance, Func<ResourceLocation, Material, TextureAtlasSprite> func, ResourceLocation location)
        {
            _instance = instance;
            _modelTextureGetter = m => func(location, m);
        }

        public IUnbakedModel GetModel(ResourceLocation location) => _instance.GetModel(location);

        public IBakedModel? Bake(ResourceLocation location, IModelState modelState)
        {
            throw new NotImplementedException();
        }
    }
}