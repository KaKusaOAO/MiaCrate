using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Client.Fonts;
using MiaCrate.Client.Graphics;
using MiaCrate.Common;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using MiaCrate.Texts;
using Mochi.Concurrent;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class FontManager : IPreparableReloadListener, IDisposable
{
    private const string FontPath = "fonts.json";
    public static readonly ResourceLocation MissingFont = new("missing");
    private static readonly FileToIdConverter _fontDefinitions = FileToIdConverter.Json("font");
    
    private readonly TextureManager _textureManager;
    private readonly Dictionary<ResourceLocation, FontSet> _fontSets = new();
    private readonly FontSet _missingFontSet;
    private readonly List<IGlyphProvider> _providersToClose = new(); 
    private Dictionary<ResourceLocation, ResourceLocation> _renames = new();

    public FontManager(TextureManager textureManager)
    {
        _textureManager = textureManager;
        _missingFontSet = new FontSet(textureManager, MissingFont);
        _missingFontSet.Reload(new List<IGlyphProvider> { new AllMissingGlyphProvider()  });
    }

    public Font CreateFont() => 
        new(location => _fontSets.GetValueOrDefault(GetActualId(location), _missingFontSet), false);

    public Font CreateFontFilterFishy() =>
        new(location => _fontSets.GetValueOrDefault(GetActualId(location), _missingFontSet), true);

    private ResourceLocation GetActualId(ResourceLocation location) => 
        _renames.GetValueOrDefault(location, location);

    public void SetRenames(Dictionary<ResourceLocation, ResourceLocation> renames)
    {
        _renames = renames;
    }

    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller preparationProfiler,
        IProfilerFiller reloadProfiler, IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        preparationProfiler.StartTick();
        preparationProfiler.EndTick();
        
        return PrepareAsync(manager, preparationExecutor)
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(p => Apply(p, reloadProfiler), reloadExecutor);
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

                return Task.WhenAll(tasks).ThenApplyAsync(() =>
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
        list.Insert(0, provider);

        var set = new HashSet<int>();
        foreach (var glyph in list.SelectMany(p => p.GetSupportedGlyphs()))
        {
            set.Add(glyph);
        }

        var reverse = new List<IGlyphProvider>(list);
        reverse.Reverse();
        
        foreach (var glyph in set)
        {
            if (glyph == ' ') continue;

            foreach (var p in reverse)
            {
                if (p.GetGlyph(glyph) != null) break;
            }
        }
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
            try
            {
                using var stream = resource.Open();
                var node = JsonNode.Parse(stream)!;
                var definitionFile = Util.GetOrThrow(
                    FontDefinitionFile.Codec.Parse(JsonOps.Instance, node),
                    m => new JsonException(m));
                var providers = definitionFile.Providers;

                for (var i = providers.Count - 1; i >= 0; i--)
                {
                    var builderId = new BuilderId(location, resource.Source.PackId, i);
                    result.Add((builderId, providers[i]));
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to load font '{location}' in {FontPath} in resourcepack: '{resource.SourcePackId}'");
                Logger.Warn(ex);
            }
        }
        
        return result;
    }

    private void Apply(Preparation preparation, IProfilerFiller profilerFiller)
    {
        foreach (var set in _fontSets.Values)
        {
            set.Dispose();
        }
        _fontSets.Clear();
        
        foreach (var provider in _providersToClose)
        {
            provider.Dispose();
        }
        _providersToClose.Clear();
        
        foreach (var (key, value) in preparation.Providers)
        {
            var set = new FontSet(_textureManager, key);
            value.Reverse();
            set.Reload(value);
            _fontSets[key] = set;
        }

        _providersToClose.AddRange(preparation.AllProviders);

        if (!_fontSets.ContainsKey(Game.DefaultFont))
            throw new InvalidOperationException("Default font failed to load");
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

    private record FontDefinitionFile(List<IGlyphProviderDefinition> Providers)
    {
        public static ICodec<FontDefinitionFile> Codec { get; } =
            RecordCodecBuilder.Create<FontDefinitionFile>(instance => instance
                .Group(
                    IGlyphProviderDefinition.Codec.ListCodec.FieldOf("providers")
                        .ForGetter<FontDefinitionFile>(f => f.Providers)
                )
                .Apply(instance, p => new FontDefinitionFile(p)));
    }
}