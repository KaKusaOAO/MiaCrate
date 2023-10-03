using System.Text;
using MiaCrate.Data;
using MiaCrate.Resources;
using MiaCrate.Extensions;
using MiaCrate.Tags;
using Mochi.Utils;

namespace MiaCrate.Core;

public class MappedRegistry<T> : IWritableRegistry<T> where T : class
{
    private readonly Lifecycle _registryLifecycle;
    private int _nextId;
    public IResourceKey<IRegistry> Key { get; }
    private readonly Dictionary<int, IReferenceHolder<T>> _byId = new();
    private readonly Dictionary<T, int> _toId = new();
    private readonly Dictionary<ResourceLocation, IReferenceHolder<T>> _byLocation = new();
    private readonly Dictionary<IResourceKey<T>, IReferenceHolder<T>> _byKey = new();
    private readonly Dictionary<T, IReferenceHolder<T>> _byValue = new();
    private readonly Dictionary<T, Lifecycle> _lifecycles = new();
    private Dictionary<ITagKey<T>, INamedHolderSet<T>> _tags = new();

    private bool _frozen;
    private Dictionary<T, IReferenceHolder<T>>? _unregisteredIntrusiveHolders;
    private List<IReferenceHolder<T>>? _holdersInOrder;
    private readonly Lookup _lookup;

    public bool IsEmpty => !_byKey.Any();
    public ISet<ResourceLocation> KeySet => _byLocation.Keys.ToHashSet();
    public IHolderOwner<T> HolderOwner => _lookup;

    public MappedRegistry(IResourceKey<IRegistry> key, Lifecycle lifecycle, bool hasIntrusiveHolders = false)
    {
        _registryLifecycle = lifecycle;
        _lookup = new Lookup(this);
        Key = key;
        
        if (hasIntrusiveHolders)
        {
            _unregisteredIntrusiveHolders = new Dictionary<T, IReferenceHolder<T>>();
        }
    }

    public IReferenceHolder<T> Register(IResourceKey<T> key, T obj, Lifecycle lifecycle) => 
        RegisterMapping(_nextId, key, obj, lifecycle);

    public virtual IReferenceHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj, Lifecycle lifecycle)
    {
        ValidateWrite(key);

        if (_byKey.ContainsKey(key))
        {
            Logger.Warn($"Adding duplicate key {key} to registry");
        }

        if (_byValue.ContainsKey(obj))
        {
            Logger.Warn($"Adding duplicate value {obj} to registry");
        }

        // Holders part
        IReferenceHolder<T>? reference;
        if (_unregisteredIntrusiveHolders != null)
        {
            if (!_unregisteredIntrusiveHolders.Remove(obj, out reference)) // _customHolderProvider(obj);
            {
                throw new Exception($"Missing intrusive holder for {key}: {obj}");    
            }
            
            reference.BindKey(key);
        }
        else
        {
            reference = _byKey.ComputeIfAbsent(key, k => 
                Holder.Reference.CreateStandalone(_lookup, k));
        }

        _byKey[key] = reference;
        _byLocation[key.Location] = reference;
        _byValue[obj] = reference;
        _byId[id] = reference;
        _toId[obj] = id;
        
        if (_nextId <= id)
        {
            _nextId = id + 1;
        }

        _lifecycles[obj] = lifecycle;
        return reference;
    }

    public virtual int GetId(T obj) => _toId[obj];
    public T? ById(int i) => i >= 0 && i < _byId.Count ? _byId[i]?.Value : null;
    public T? Get(IResourceKey<T> key) => _byKey.GetValueOrDefault(key)?.Value;
    public virtual T? Get(ResourceLocation location) => _byLocation.GetValueOrDefault(location)?.Value;

    public virtual ResourceLocation? GetKey(T obj) => _byValue.GetValueOrDefault(obj)?.Key.Location;
    
    public IOptional<IResourceKey<T>> GetResourceKey(T obj) => 
        Optional.OfNullable(_byValue.GetValueOrDefault(obj)).Select(h => h.Key);

    public Lifecycle Lifecycle(T obj) => _lifecycles[obj];

    public IOptional<IReferenceHolder<T>> GetHolder(int id) => 
        id >= 0 && id < _byId.Count ? Optional.OfNullable(_byId[id]) : Optional.Empty<IReferenceHolder<T>>();

    public IOptional<IReferenceHolder<T>> GetHolder(IResourceKey<T> key) => 
        Optional.OfNullable(_byKey[key]);

    public IReferenceHolder<T> CreateIntrusiveHolder(T obj)
    {
        if (_unregisteredIntrusiveHolders == null)
        {
            throw new InvalidOperationException("This registry can't create intrusive holders");
        }

        return _unregisteredIntrusiveHolders
            .ComputeIfAbsent(obj, o => Holder.Reference.CreateIntrusive(_lookup, o));
    }
    
    private List<IReferenceHolder<T>> HoldersInOrder() =>
        _holdersInOrder ??= _byId.ToList()
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();

    public IEnumerator<T> GetEnumerator() => HoldersInOrder()
        .Select(x => x.Value)
        .GetEnumerator();
    
    public IHolderLookup<T> AsLookup() => _lookup;

    public IRegistry<T> Freeze()
    {
        if (_frozen) return this;

        _frozen = true;
        foreach (var (val, holder) in _byValue)
        {
            holder.BindValue(val);
        }

        var list = _byKey.Where(x => !x.Value.IsBound)
            .Select(x => x.Key.Location)
            .OrderBy(x => x.ToString())
            .ToList();
        if (list.Any())
            throw new Exception($"Unbound values in registry {Key}: {list}");

        if (_unregisteredIntrusiveHolders != null)
        {
            if (_unregisteredIntrusiveHolders.Any())
                throw new Exception(
                    $"Some intrusive holders were not registered: {_unregisteredIntrusiveHolders.Values}");
            _unregisteredIntrusiveHolders = null;
        }

        return this;
    }

    public IHolderGetter CreateRegistrationLookup()
    {
        ValidateWrite();
        return new Getter(this);
    }
    
    private void ValidateWrite(IResourceKey<T>? key = null)
    {
        if (!_frozen) return;
        
        var sb = new StringBuilder("Registry is already frozen");
        if (key != null)
            sb.Append($" (trying to add key {key})");
        
        throw new InvalidOperationException(sb.ToString());
    }

    private IReferenceHolder<T> GetOrCreateHolderOrThrow(IResourceKey<T> key)
    {
        return _byKey.ComputeIfAbsent(key, k =>
        {
            if (_unregisteredIntrusiveHolders != null)
                throw new InvalidOperationException("This registry can't create new holders without value");

            ValidateWrite(k);
            return Holder.Reference.CreateStandalone(HolderOwner, k);
        });
    }

    private INamedHolderSet<T> GetOrCreateTag(ITagKey<T> tagKey)
    {
        var named = _tags.GetValueOrDefault(tagKey);
        if (named != null) return named;
        
        var tag = CreateTag(tagKey);

        // Why?
        _tags = new Dictionary<ITagKey<T>, INamedHolderSet<T>>(_tags)
        {
            [tagKey] = tag
        };

        return tag;
    }

    private INamedHolderSet<T> CreateTag(ITagKey<T> tagKey) => HolderSet.CreateNamed(HolderOwner, tagKey);

    private IOptional<INamedHolderSet<T>> GetTag(ITagKey<T> tagKey) => 
        Optional.OfNullable(_tags.GetValueOrDefault(tagKey));

    private class Getter : IHolderGetter<T>
    {
        private readonly MappedRegistry<T> _inner;

        public Getter(MappedRegistry<T> inner)
        {
            _inner = inner;
        }

        public IOptional<IReferenceHolder<T>> Get(IResourceKey<T> key) => Optional.Of(GetOrThrow(key));
        
        public IReferenceHolder<T> GetOrThrow(IResourceKey<T> key) => _inner.GetOrCreateHolderOrThrow(key);
        public IOptional<INamedHolderSet<T>> Get(ITagKey<T> tagKey) => Optional.Of(GetOrThrow(tagKey));
        public INamedHolderSet<T> GetOrThrow(ITagKey<T> tagKey) => _inner.GetOrCreateTag(tagKey);
    }

    private class Lookup : IRegistryLookup<T>
    {
        private readonly MappedRegistry<T> _registry;

        public Lookup(MappedRegistry<T> registry)
        {
            _registry = registry;
        }

        public IResourceKey<IRegistry> Key => _registry.Key;
        public IOptional<IReferenceHolder<T>> Get(IResourceKey<T> key) => _registry.GetHolder(key);
        public IOptional<INamedHolderSet<T>> Get(ITagKey<T> tagKey) => _registry.GetTag(tagKey);
    }
}