using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using MiaCrate.Data;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using Mochi.Concurrent;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class FontManager : IPreparableReloadListener, IDisposable
{
    public static readonly ResourceLocation MissingFont = new("missing");
    private static readonly FileToIdConverter _fontDefinitions = FileToIdConverter.Json("font");
    
    private readonly TextureManager _textureManager;
    private readonly Dictionary<ResourceLocation, FontSet> _fontSets = new();
    private readonly Dictionary<ResourceLocation, ResourceLocation> _renames = new();
    private readonly FontSet _missingFontSet;

    public FontManager(TextureManager textureManager)
    {
        _textureManager = textureManager;
        _missingFontSet = new FontSet(textureManager, MissingFont);
        _missingFontSet.Reload(new List<IGlyphProvider> {  });
    }

    public Font CreateFont() => 
        new(location => _fontSets.GetValueOrDefault(GetActualId(location), _missingFontSet), false);

    private ResourceLocation GetActualId(ResourceLocation location) => 
        _renames.GetValueOrDefault(location, location);

    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller profiler,
        IProfilerFiller profiler2, IExecutor executor, IExecutor executor2)
    {
        profiler.StartTick();
        profiler.EndTick();
        
        return PrepareAsync(manager, executor)
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(p => Apply(p, profiler2), executor2);
    }

    private Task<Preparation> PrepareAsync(IResourceManager resourceManager, IExecutor executor)
    {
        var list = new List<Task<UnresolvedBuilderBundle>>();
        foreach (var (key, value) in _fontDefinitions.ListMatchingResourceStacks(resourceManager))
        {
            var location = _fontDefinitions.FileToId(key);
            list.Add(Tasks.SupplyAsync(() => { 
                var l = LoadResourceStack(value, location);
                var bundle = new UnresolvedBuilderBundle(location);
                foreach (var (builderId, definition) in l)
                {
                    definition.Unpack().IfLeft(loader =>
                    {
                        var task = SafeLoadAsync(builderId, loader, resourceManager, executor);
                        bundle.Add(builderId, task);
                    }).IfRight(reference =>
                    {
                        bundle.Add(builderId, reference);
                    });
                }

                return bundle;
            }, executor));
        }

        return Util.Sequence(list).ThenComposeAsync(listX =>
        {
            var list2 = listX.SelectMany(b => b.ListBuilders()).ToList();
            var provider = new AllMissingGlyphProvider();
            list2.Add(Task.FromResult(Optional.Of<IGlyphProvider>(provider)));

            return Util.Sequence(list2).ThenComposeAsync(providers =>
            {
                var dict = ResolveProviders(listX);
                var tasks = dict.Values.Select(ps =>
                    Tasks.RunAsync(() => FinalizeProviderLoading(ps, provider), executor));

                return Task.WhenAll(tasks).ContinueWith(_ =>
                {
                    var ps = providers.SelectMany(o => o.AsEnumerable()).ToList();
                    return new Preparation(dict, ps);
                });
            });
        });
    }

    private Dictionary<ResourceLocation, List<IGlyphProvider>> ResolveProviders(List<UnresolvedBuilderBundle> bundles)
    {
        var dict = new Dictionary<ResourceLocation, List<IGlyphProvider>>();
        var sorter = new DependencySorter<ResourceLocation, UnresolvedBuilderBundle>();
        foreach (var bundle in bundles)
        {
            sorter.AddEntry(bundle.FontId, bundle);
        }
        
        sorter.OrderByDependencies((location, bundle) =>
        {
            bundle.Resolve(l => dict.GetValueOrDefault(l)).IfPresent(l =>
            {
                dict[location] = l;
            });
        });

        return dict;
    }

    private void FinalizeProviderLoading(List<IGlyphProvider> list, IGlyphProvider provider)
    {
        throw new NotImplementedException();
    }

    private Task<IOptional<IGlyphProvider>> SafeLoadAsync(BuilderId builderId, IGlyphProviderDefinition.ILoader loader,
        IResourceManager manager, IExecutor executor)
    {
        var source = new TaskCompletionSource<IOptional<IGlyphProvider>>();
        executor.Execute(() =>
        {
            try
            {
                source.SetResult(Optional.Of(loader.Load(manager)));
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load builder {builderId}, rejecting");
                Logger.Warn(ex);
                source.SetResult(Optional.Empty<IGlyphProvider>());
            }
        });

        return source.Task;
    }

    private static List<(BuilderId, IGlyphProviderDefinition)> LoadResourceStack(List<Resource> resources,
        ResourceLocation location)
    {
        var result = new List<(BuilderId, IGlyphProviderDefinition)>();
        foreach (var resource in resources)
        {
            using var stream = resource.Open();
            throw new NotImplementedException();
        }
        
        return result;
    }

    private void Apply(Preparation preparation, IProfilerFiller profilerFiller)
    {
        
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private record Preparation(Dictionary<ResourceLocation, List<IGlyphProvider>> Providers,
        List<IGlyphProvider> AllProviders);

    private record BuilderId(ResourceLocation FontId, string Pack, int Index);

    private record BuilderResult(BuilderId Id, IEither<Task<IOptional<IGlyphProvider>>, ResourceLocation> Result)
    {
        public IOptional<List<IGlyphProvider>> Resolve(Func<ResourceLocation, List<IGlyphProvider>?> func)
        {
            return Result.Select(
                t => t.Result.Select(r => new List<IGlyphProvider> {r}), 
                location =>
                {
                    var list = func(location);
                    if (list != null) return Optional.Of(list);
                    
                    Logger.Warn($"Can't find font {location} referenced by builder {Id}, " +
                                "either because it's missing, failed to load or is part of loading cycle");
                    return Optional.Empty<List<IGlyphProvider>>();
                });
        }
    }

    private record UnresolvedBuilderBundle(ResourceLocation FontId, List<BuilderResult> Builders,
        HashSet<ResourceLocation> Dependencies) : IDependencySorterEntry<ResourceLocation>
    {
        public UnresolvedBuilderBundle(ResourceLocation location)
            : this(location, new List<BuilderResult>(), new HashSet<ResourceLocation>()) {}

        public void Add(BuilderId builderId, IGlyphProviderDefinition.Reference reference)
        {
            Builders.Add(new BuilderResult(builderId, Either
                .CreateRight(reference.Id)
                .Left<Task<IOptional<IGlyphProvider>>>()
            ));
            Dependencies.Add(reference.Id);
        }

        public void Add(BuilderId builderId, Task<IOptional<IGlyphProvider>> task)
        {
            Builders.Add(new BuilderResult(builderId, Either
                .CreateLeft(task)
                .Right<ResourceLocation>()
            ));
        }

        public IEnumerable<Task<IOptional<IGlyphProvider>>> ListBuilders() =>
            Builders.SelectMany(r => r.Result.Left.AsEnumerable());

        public IOptional<List<IGlyphProvider>> Resolve(Func<ResourceLocation, List<IGlyphProvider>?> func)
        {
            var list = new List<IGlyphProvider>();
            foreach (var builderResult in Builders)
            {
                var opt = builderResult.Resolve(func);
                if (opt.IsEmpty) return Optional.Empty<List<IGlyphProvider>>(); // ?
                list.AddRange(opt.Value);
            }

            return Optional.Of(list);
        }

        public void VisitRequiredDependencies(Action<ResourceLocation> consumer)
        {
            foreach (var resourceLocation in Dependencies)
            {
                consumer(resourceLocation);
            }
        }

        public void VisitOptionalDependencies(Action<ResourceLocation> consumer)
        {
            
        }
    }
}