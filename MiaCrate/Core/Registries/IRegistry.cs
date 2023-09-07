using MiaCrate.Data;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IRegistry : IKeyable
{
    public IResourceKey<IRegistry> Key { get; }
    public int GetId(object obj);
    public object? ById(int id);
    public object? Get(IResourceKey key);
    public object? Get(ResourceLocation location);
    public ResourceLocation? GetKey(object obj);
    public ISet<ResourceLocation> KeySet { get; }
    public IRegistry Freeze();

    IEnumerable<T> IKeyable.GetKeys<T>(IDynamicOps<T> ops) => 
        KeySet.Select(r => ops.CreateString(r));
}

public interface IRegistry<T> : IRegistry, IIdMap<T> where T : class
{
    public int GetId(T obj);
    int IRegistry.GetId(object obj) => GetId((T) obj);

    public new T? ById(int id);
    object? IRegistry.ById(int id) => ById(id);
    T? IIdMap<T>.ById(int id) => ById(id);
    
    public T? Get(IResourceKey<T> key);
    object? IRegistry.Get(IResourceKey key) => Get((IResourceKey<T>) key);
    
    public new T? Get(ResourceLocation location);
    object? IRegistry.Get(ResourceLocation location) => Get(location);

    public ResourceLocation? GetKey(T obj);
    ResourceLocation? IRegistry.GetKey(object obj) => GetKey((T) obj);

    public IOptional<IReferenceHolder<T>> GetHolder(int id);
    public IOptional<IReferenceHolder<T>> GetHolder(IResourceKey<T> key);
    public new IRegistry<T> Freeze();
    IRegistry IRegistry.Freeze() => Freeze();
    public IReferenceHolder<T> CreateIntrusiveHolder(T obj);
    
    public IHolderOwner<T> HolderOwner { get; }
    public IHolderLookup<T> AsLookup();
}