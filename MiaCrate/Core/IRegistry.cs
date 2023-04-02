using MiaCrate.Resources;

namespace MiaCrate.Core;

public interface IRegistry
{
    public IResourceKey<IRegistry> Key { get; }
    public int GetId(object obj);
    public object? ById(int id);
    public object? Get(IResourceKey key);
    public object? Get(ResourceLocation location);
    public ResourceLocation? GetKey(object obj);
    public ISet<ResourceLocation> KeySet { get; }
}

public interface IRegistry<T> : IRegistry where T : class
{
    public int GetId(T obj);
    int IRegistry.GetId(object obj) => GetId((T) obj);

    public new T? ById(int id);
    object? IRegistry.ById(int id) => ById(id);
    
    public T? Get(IResourceKey<T> key);
    object? IRegistry.Get(IResourceKey key) => Get((IResourceKey<T>) key);
    
    public new T? Get(ResourceLocation location);
    object? IRegistry.Get(ResourceLocation location) => Get(location);

    public ResourceLocation? GetKey(T obj);
    ResourceLocation? IRegistry.GetKey(object obj) => GetKey((T) obj);

    public IReferenceHolder<T> CreateIntrusiveHolder(T obj);
}