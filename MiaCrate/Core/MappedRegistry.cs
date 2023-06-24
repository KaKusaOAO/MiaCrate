using MiaCrate.Resources;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Core;

public class MappedRegistry<T> : WritableRegistry<T> where T : class
{
    private int _nextId;
    private readonly IResourceKey<IRegistry> _key;
    private readonly Dictionary<int, IReferenceHolder<T>> _byId = new();
    private readonly Dictionary<T, int> _toId = new();
    private readonly Dictionary<IResourceKey<T>, IReferenceHolder<T>> _byKey = new();
    private readonly Dictionary<T, IReferenceHolder<T>> _byValue = new();
    private readonly Dictionary<ResourceLocation, IReferenceHolder<T>> _byLocation = new();
    private readonly Dictionary<T, IReferenceHolder<T>> _intrusiveHolderCache = new();
    private Dictionary<T, IReferenceHolder<T>>? _unregisteredIntrusiveHolders;

    public MappedRegistry(IResourceKey<IRegistry> key, bool hasIntrusiveHolders = false) : base(key)
    {
        _key = key;
        
        if (hasIntrusiveHolders)
        {
            _unregisteredIntrusiveHolders = new Dictionary<T, IReferenceHolder<T>>();
        }
    }

    public override IHolder<T> Register(IResourceKey<T> key, T obj) => 
        RegisterMapping(_nextId, key, obj);

    public override IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj) => 
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
                ReferenceHolder<T>.CreateStandalone(this, k));
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

    public override int GetId(T obj) => _toId[obj];
    public override T? ById(int i) => i >= 0 && i < _byId.Count ? _byId[i]?.Value : null;
    public override T? Get(IResourceKey<T> key) => _byKey.GetValueOrDefault(key)?.Value;
    public override T? Get(ResourceLocation location) => _byLocation.GetValueOrDefault(location)?.Value;

    public override ResourceLocation? GetKey(T obj) => _byValue.GetValueOrDefault(obj)?.Key.Location;
    public override IReferenceHolder<T> CreateIntrusiveHolder(T obj)
    {
        if (_unregisteredIntrusiveHolders == null)
        {
            throw new InvalidOperationException("This registry can't create intrusive holders");
        }

        return _intrusiveHolderCache.ComputeIfAbsent(obj, o => ReferenceHolder<T>.CreateIntrusive(this, o));
    }

    public override ISet<ResourceLocation> KeySet => _byLocation.Keys.ToHashSet();
}