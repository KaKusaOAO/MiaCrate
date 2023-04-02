using MiaCrate.Resources;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Core;

public class MappedRegistry<T> : WritableRegistry<T> where T : class
{
    private int _nextId;
    private Dictionary<int, IReferenceHolder<T>> _byId = new();
    private Dictionary<T, int> _toId = new();
    private Dictionary<IResourceKey<T>, IReferenceHolder<T>> _byKey = new();
    private Dictionary<T, IReferenceHolder<T>> _byValue = new();
    private Dictionary<ResourceLocation, IReferenceHolder<T>> _byLocation = new();
    private Dictionary<T, IReferenceHolder<T>> _intrusiveHolderCache = new();
    private Func<T, IReferenceHolder<T>>? _customHolderProvider;

    public MappedRegistry(IResourceKey<IRegistry> key, Func<T, IReferenceHolder<T>>? provider) : base(key)
    {
        _customHolderProvider = provider;
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
        if (_customHolderProvider != null)
        {
            reference = _customHolderProvider(obj);
            var r = _byKey.AddOrSet(key, reference);
            if (r != null && r != reference)
            {
                throw new InvalidOperationException($"Invalid holder present for key {key}");
            }
        }
        else
        {
            reference = _byKey.ComputeIfAbsent(key, k => 
                ReferenceHolder<T>.CreateStandalone(this, k));
        }
        
        _byLocation.Add(key.Location, reference);
        _byValue.Add(obj, reference);
        reference.Bind(key, obj);
        _byId.Add(id, reference);
        return reference;
    }

    public override int GetId(T obj) => _toId[obj];
    public override T? ById(int i) => i >= 0 && i < _byId.Count ? _byId[i]?.Value : null;
    public override T? Get(IResourceKey<T> key) => _byKey.GetValueOrDefault(key)?.Value;
    public override T? Get(ResourceLocation location) => _byLocation.GetValueOrDefault(location)?.Value;

    public override ResourceLocation? GetKey(T obj) => _byValue.GetValueOrDefault(obj)?.Key.Location;
    public override IReferenceHolder<T> CreateIntrusiveHolder(T obj)
    {
        if (_customHolderProvider == null)
        {
            throw new InvalidOperationException("This registry can't create intrusive holders");
        }

        return _intrusiveHolderCache.ComputeIfAbsent(obj, o => ReferenceHolder<T>.CreateIntrusive(this, o));
    }

    public override ISet<ResourceLocation> KeySet => _byLocation.Keys.ToHashSet();
}