using MiaCrate.Resources;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Core;

public class MappedRegistry<T> : IWritableRegistry<T> where T : class
{
    private int _nextId;
    public IResourceKey<IRegistry> Key { get; }
    private readonly Dictionary<int, IReferenceHolder<T>> _byId = new();
    private readonly Dictionary<T, int> _toId = new();
    private readonly Dictionary<IResourceKey<T>, IReferenceHolder<T>> _byKey = new();
    private readonly Dictionary<T, IReferenceHolder<T>> _byValue = new();
    private readonly Dictionary<ResourceLocation, IReferenceHolder<T>> _byLocation = new();
    private bool _frozen;
    private Dictionary<T, IReferenceHolder<T>>? _unregisteredIntrusiveHolders;
    private List<IReferenceHolder<T>>? _holdersInOrder;
    private readonly Lookup _lookup;

    public MappedRegistry(IResourceKey<IRegistry> key, bool hasIntrusiveHolders = false)
    {
        _lookup = new Lookup(this);
        Key = key;
        
        if (hasIntrusiveHolders)
        {
            _unregisteredIntrusiveHolders = new Dictionary<T, IReferenceHolder<T>>();
        }
    }

    public IHolder<T> Register(IResourceKey<T> key, T obj) => 
        RegisterMapping(_nextId, key, obj);

    public virtual IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj) => 
        RegisterMapping(id, key, obj, true);

    private IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj, bool checkKey)
    {
        _toId.Add(obj, id);

        if (checkKey && _byKey.ContainsKey(key))
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
                ReferenceHolder<T>.CreateStandalone(_lookup, k));
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
        
        return reference;
    }

    public virtual int GetId(T obj) => _toId[obj];
    public T? ById(int i) => i >= 0 && i < _byId.Count ? _byId[i]?.Value : null;
    public T? Get(IResourceKey<T> key) => _byKey.GetValueOrDefault(key)?.Value;
    public virtual T? Get(ResourceLocation location) => _byLocation.GetValueOrDefault(location)?.Value;

    public virtual ResourceLocation? GetKey(T obj) => _byValue.GetValueOrDefault(obj)?.Key.Location;
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
            .ComputeIfAbsent(obj, o => ReferenceHolder<T>.CreateIntrusive(_lookup, o));
    }

    public ISet<ResourceLocation> KeySet => _byLocation.Keys.ToHashSet();

    private List<IReferenceHolder<T>> HoldersInOrder() =>
        _holdersInOrder ??= _byId.ToList()
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();

    public IEnumerator<T> GetEnumerator() => HoldersInOrder()
        .Select(x => x.Value)
        .GetEnumerator();

    public IHolderOwner<T> HolderOwner => _lookup;
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

    private class Lookup : IRegistryLookup<T>
    {
        private readonly MappedRegistry<T> _registry;

        public Lookup(MappedRegistry<T> registry)
        {
            _registry = registry;
        }

        public IResourceKey<IRegistry> Key => _registry.Key;
        public IOptional<IReferenceHolder<T>> Get(IResourceKey<T> key) => _registry.GetHolder(key);
    }
}