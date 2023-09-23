using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class FallbackResourceManager : IResourceManager
{
    private readonly List<PackEntry> _fallbacks = new();
    private readonly PackType _type;
    private readonly string _namespace;

    public FallbackResourceManager(PackType type, string ns)
    {
        _type = type;
        _namespace = ns;
    }

    public void Push(IPackResources resources) => 
        PushInternal(resources.PackId, resources, null);

    public void Push(IPackResources resources, Predicate<ResourceLocation>? filter) => 
        PushInternal(resources.PackId, resources, filter);

    public void PushFilterOnly(string name, Predicate<ResourceLocation>? filter) => 
        PushInternal(name, null, filter);

    private void PushInternal(string name, IPackResources? resources, Predicate<ResourceLocation>? filter) => 
        _fallbacks.Add(new PackEntry(name, resources, filter));

    public IOptional<Resource> GetResource(ResourceLocation location)
    {
        for (var i = 0; i < _fallbacks.Count; i++)
        {
            var entry = _fallbacks[i];
            var resources = entry.Resources;
            if (resources != null)
            {
                var func = resources.GetResource(_type, location);
                if (func != null)
                {
                    var func2 = CreateStackMetadataFinder(location, i);
                    return Optional.Of(CreateResource(resources, location, func, func2));
                }
            }

            if (entry.IsFiltered(location))
            {
                Logger.Warn($"Resource {location} not found, but was filtered by pack {entry.Name}");
                return Optional.Empty<Resource>();
            }
        }

        return Optional.Empty<Resource>();
    }

    private static Resource CreateResource(IPackResources resources, ResourceLocation location, Func<Stream> stream,
        Func<IResourceMetadata> metadata) => 
        new(resources, WrapForDebug(location, resources, stream), metadata);

    public static Func<Stream> WrapForDebug(ResourceLocation location, IPackResources resources, Func<Stream> stream) => 
        () => new LeakedResourceWarningStream(stream(), location, resources.PackId);

    private Func<IResourceMetadata> CreateStackMetadataFinder(ResourceLocation location, int index)
    {
        return () =>
        {
            var l = GetMetadataLocation(location);

            for (var j = _fallbacks.Count - 1; j >= index; j--)
            {
                var entry = _fallbacks[j];
                var resources = entry.Resources;
                if (resources != null)
                {
                    var func = resources.GetResource(_type, l);
                    if (func != null)
                    {
                        return ParseMetadata(func);
                    }
                }

                if (entry.IsFiltered(l)) break;
            }
            
            return IResourceMetadata.Empty;
        };
    }

    private static IResourceMetadata ParseMetadata(Func<Stream> func)
    {
        using var stream = func();
        return IResourceMetadata.FromJsonStream(stream);
    }

    public HashSet<string> Namespaces => new() {_namespace};

    public List<Resource> GetResourceStack(ResourceLocation location)
    {
        var l = GetMetadataLocation(location);
        var list = new List<Resource>();
        var bl = false;
        string? str = null;
        
        for (var i = _fallbacks.Count - 1; i >= 0; i--)
        {
            var entry = _fallbacks[i];
            var resources = entry.Resources;
            if (resources != null)
            {
                var stream = resources.GetResource(_type, location);
                if (stream != null)
                {
                    var metadata = bl
                        ? IResourceMetadata.EmptySupplier
                        : () =>
                        {
                            var s = resources.GetResource(_type, l);
                            return s != null
                                ? ParseMetadata(s)
                                : IResourceMetadata.Empty;
                        };
                    list.Add(new Resource(resources, stream, metadata));
                }
            }

            if (entry.IsFiltered(location))
            {
                str = entry.Name;
                break;
            }

            if (entry.IsFiltered(l))
            {
                bl = true;
            }
        }

        if (!list.Any() && str != null)
        {
            Logger.Warn($"Resource {location} not found, but was filtered by pack {str}");
        }

        var reversed = new List<Resource>(list);
        reversed.Reverse();
        return reversed;
    }

    public Dictionary<ResourceLocation, Resource> ListResources(string path, Predicate<ResourceLocation> predicate)
    {
        var dict = new Dictionary<ResourceLocation, ResourceWithSourceAndIndex>();
        var dict2 = new Dictionary<ResourceLocation, ResourceWithSourceAndIndex>();
        var i = _fallbacks.Count;

        for (var j = 0; j < i; j++)
        {
            var entry = _fallbacks[j];

            var keys = dict.Keys.ToList();
            var keys2 = dict2.Keys.ToList();
            entry.FilterAll(keys);
            entry.FilterAll(keys2);

            var removedKeys = dict.Keys.ToList();
            removedKeys.RemoveAll(e => keys.Contains(e));
            var removedKeys2 = dict2.Keys.ToList();
            removedKeys2.RemoveAll(e => keys2.Contains(e));
            
            foreach (var resourceLocation in removedKeys)
            {
                dict.Remove(resourceLocation);
            }
            
            foreach (var resourceLocation in removedKeys2)
            {
                dict.Remove(resourceLocation);
            }

            var resources = entry.Resources;
            var jx = j;
            resources?.ListResources(_type, _namespace, path, (location, stream) =>
            {
                if (IsMetadata(location))
                {
                    if (predicate(GetResourceLocationFromMetadata(location)))
                    {
                        dict2[location] = new ResourceWithSourceAndIndex(resources, stream, jx);
                    }
                }
                else if (predicate(location))
                {
                    dict[location] = new ResourceWithSourceAndIndex(resources, stream, jx);
                }
            });
        }

        var result = new Dictionary<ResourceLocation, Resource>();
        foreach (var (key, value) in dict)
        {
            var l = GetMetadataLocation(key);
            var lv = dict2.GetValueOrDefault(l);
            var stream = lv != null && lv.PackIndex >= value.PackIndex
                ? ConvertToMetadata(lv.Resource)
                : IResourceMetadata.EmptySupplier;
            result[key] = CreateResource(value.PackResources, key, value.Resource, stream);
        }

        return result;
    }

    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string path, Predicate<ResourceLocation> predicate)
    {
        var dict = new Dictionary<ResourceLocation, EntryStack>();
        
        foreach (var packEntry in _fallbacks)
        {
            ApplyPackFiltersToExistingResources(packEntry, dict);
            ListPackResources(packEntry, path, predicate, dict);
        }

        var tree = new Dictionary<ResourceLocation, List<Resource>>();
        foreach (var entryStack in dict.Values)
        {
            var list = new List<Resource>();
            
            foreach (var record in entryStack.FileSources)
            {
                var resources = record.Source;
                var stream = entryStack.MetaSources.GetValueOrDefault(resources);
                var metadata = stream != null
                    ? ConvertToMetadata(stream)
                    : IResourceMetadata.EmptySupplier;
                list.Add(CreateResource(resources, entryStack.FileLocation, record.Resource, metadata));
            }

            tree[entryStack.FileLocation] = list;
        }

        return tree
            .OrderBy(e => e.Key)
            .ToDictionary(e => e.Key, e => e.Value);
    }

    private static Func<IResourceMetadata> ConvertToMetadata(Func<Stream> stream) =>
        () => ParseMetadata(stream);

    private void ListPackResources(PackEntry entry, string path, Predicate<ResourceLocation> predicate,
        Dictionary<ResourceLocation, EntryStack> dict)
    {
        var resources = entry.Resources;
        resources?.ListResources(_type, _namespace, path, (location, stream) =>
        {
            if (IsMetadata(location))
            {
                var l = GetResourceLocationFromMetadata(location);
                if (!predicate(l)) return;
                dict.ComputeIfAbsent(l, r => new EntryStack(r)).MetaSources[resources] = stream;
            }
            else
            {
                if (!predicate(location)) return;
                dict.ComputeIfAbsent(location, r => new EntryStack(r))
                    .FileSources.Add(new ResourceWithSource(resources, stream));
            }
        });
    }

    private static ResourceLocation GetResourceLocationFromMetadata(ResourceLocation location)
    {
        var path = location.Path[..^".mcmeta".Length];
        return location.WithPath(path);
    }

    private static bool IsMetadata(ResourceLocation location) =>
        location.Path.EndsWith(".mcmeta");
    
    private static void ApplyPackFiltersToExistingResources(PackEntry entry,
        Dictionary<ResourceLocation, EntryStack> dict)
    {
        foreach (var entryStack in dict.Values)
        {
            if (entry.IsFiltered(entryStack.FileLocation))
            {
                entryStack.FileSources.Clear();
            } else if (entry.IsFiltered(entryStack.MetadataLocation))
            {
                entryStack.MetaSources.Clear();
            }
        }
    }

    public IEnumerable<IPackResources> Packs => throw new NotImplementedException();

    private static ResourceLocation GetMetadataLocation(ResourceLocation location) => 
        location.WithPath(location.Path + ".mcmeta");

    private record PackEntry(string Name, IPackResources? Resources, Predicate<ResourceLocation>? Filter)
    {
        public void FilterAll(List<ResourceLocation> locations)
        {
            if (Filter == null) return;
            locations.RemoveAll(Filter);
        }

        public bool IsFiltered(ResourceLocation location) => Filter != null && Filter(location);
    }
    
    private record EntryStack(ResourceLocation FileLocation, ResourceLocation MetadataLocation,
        List<ResourceWithSource> FileSources, Dictionary<IPackResources, Func<Stream>> MetaSources)
    {
        public EntryStack(ResourceLocation location)
            : this(location, GetMetadataLocation(location), new List<ResourceWithSource>(),
                new Dictionary<IPackResources, Func<Stream>>()) {}
    }

    private record ResourceWithSource(IPackResources Source, Func<Stream> Resource);

    private record ResourceWithSourceAndIndex(IPackResources PackResources, Func<Stream> Resource, int PackIndex);

    private class LeakedResourceWarningStream : Stream
    {
        private readonly Func<string> _message;
        private readonly Stream _inner;
        private bool _closed;

        public LeakedResourceWarningStream(Stream inner, ResourceLocation location, string pack)
        {
            _inner = inner;

            var stackTrace = Environment.StackTrace;
            _message = () => $"Leaked resource: '{location}' loaded from pack: '{pack}'\n{stackTrace}";
        }

        ~LeakedResourceWarningStream()
        {
            if (!_closed)
            {
                Logger.Warn(_message());
            }
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            _inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) =>
            _inner.Seek(offset, origin);

        public override void SetLength(long value) =>
            _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            _inner.Write(buffer, offset, count);
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _closed = true;
        }
    }
}