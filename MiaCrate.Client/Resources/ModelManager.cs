using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Client.Colors;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Models;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using MiaCrate.World.Blocks;
using Mochi.Utils;

namespace MiaCrate.Client.Resources;

public class ModelManager : IPreparableReloadListener, IDisposable
{
    private static readonly Dictionary<ResourceLocation, ResourceLocation> _vanillaAtlases = new()
    {
        [Sheets.BannerSheet] = new("banner_patterns"),
        [Sheets.BedSheet] = new("beds"),
        [Sheets.ChestSheet] = new("chests"),
        [Sheets.ShieldSheet] = new("shield_patterns")
    };

    private readonly BlockColors _blockColors;
    private readonly int _maxMipmapLevels;
    private readonly AtlasSet _atlases;
    
    public BlockModelShaper BlockModelShaper { get; }
    
    public ModelManager(TextureManager textureManager, BlockColors blockColors, int maxMipmapLevels)
    {
        _blockColors = blockColors;
        _maxMipmapLevels = maxMipmapLevels;
        BlockModelShaper = new BlockModelShaper(this);
        _atlases = new AtlasSet(_vanillaAtlases, textureManager);
    }

    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller profiler,
        IProfilerFiller profiler2, IExecutor executor, IExecutor executor2)
    {
        profiler.StartTick();
        var task = LoadBlockModelsAsync(manager, executor);
        var task2 = LoadBlockStatesAsync(manager, executor);
        var task3 = task.ThenCombineAsync(task2, (dict, dict2) => 
            new ModelBakery(_blockColors, profiler, dict, dict2), executor);

        var dict = _atlases.ScheduleLoad(manager, _maxMipmapLevels, executor);
        return Task
            .WhenAll(dict.Values.Concat(new List<Task> {task3}))
            .ThenApplyAsync(() => LoadModels(profiler,
                dict.ToDictionary(e => e.Key, e => e.Value.Result),
                task3.Result), executor)
            .ThenComposeAsync(state => state.ReadyForUpload.ThenApplyAsync(() => state))
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(s => Apply(s, profiler2), executor2);
    }

    private void Apply(ReloadState state, IProfilerFiller profiler)
    {
        profiler.StartTick();
        profiler.Push("upload");
        foreach (var result in state.AtlasPreparations.Values)
        {
            result.Upload();
        }
        
        profiler.PopPush("cache");
        
        profiler.Pop();
        profiler.EndTick();
    }

    private ReloadState LoadModels(IProfilerFiller profiler, Dictionary<ResourceLocation, AtlasSet.StitchResult> dict,
        ModelBakery modelBakery)
    {
        throw new NotImplementedException();
    }

    private record ReloadState(ModelBakery ModelBakery, IBakedModel MissingModel,
        Dictionary<BlockState, IBakedModel> ModelCache,
        Dictionary<ResourceLocation, AtlasSet.StitchResult> AtlasPreparations, Task ReadyForUpload);

    private static Task<Dictionary<ResourceLocation, BlockModel>> LoadBlockModelsAsync(IResourceManager manager,
        IExecutor executor)
    {
        return Tasks
            .SupplyAsync(() => ModelBakery.ModelLister.ListMatchingResources(manager), executor)
            .ThenComposeAsync(map => 
            {
                var list = new List<Task<(ResourceLocation, BlockModel)?>>();

                foreach (var (key, resource) in map)
                {
                    list.Add(Tasks.SupplyAsync<(ResourceLocation, BlockModel)?>(() =>
                    {
                        try
                        {
                            using var stream = resource.Open();
                            return (key, BlockModel.FromStream(stream));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to load model {key}");
                            Logger.Error(ex);
                            return null;
                        }
                    }, executor));
                }

                return Util.Sequence(list)
                    .ThenApplyAsync(lx => lx
                        .Where(v => v.HasValue)
                        .Select(v => v!.Value)
                        .ToDictionarySkipDuplicates(
                            v => v.Item1, 
                            v => v.Item2)
                        );
            });
    }

    private static Task<Dictionary<ResourceLocation, List<ModelBakery.LoadedJson>>> LoadBlockStatesAsync(IResourceManager manager,
        IExecutor executor)
    {
        return Tasks
            .SupplyAsync(() => ModelBakery.BlockStateLister.ListMatchingResourceStacks(manager), executor)
            .ThenComposeAsync(map =>
            {
                var list = new List<Task<(ResourceLocation, List<ModelBakery.LoadedJson>)>>();
                foreach (var (location, resources) in map)
                {
                    list.Add(Tasks.SupplyAsync(() =>
                    {
                        var loaded = new List<ModelBakery.LoadedJson>();
                        foreach (var resource in resources)
                        {
                            try
                            {
                                using var stream = resource.Open();
                                var node = JsonSerializer.Deserialize<JsonObject>(stream)!;
                                loaded.Add(new ModelBakery.LoadedJson(resource.SourcePackId, node));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Failed to load blockstate {location} from pack {resource.SourcePackId}");
                                Logger.Error(ex);
                            }
                        }

                        return (location, loaded);
                    }, executor));
                }

                return Util
                    .Sequence(list)
                    .ThenApplyAsync(lx => lx.ToDictionary(v => v.Item1, v => v.Item2));
            });
    }

    public void Dispose()
    {
        
    }
}