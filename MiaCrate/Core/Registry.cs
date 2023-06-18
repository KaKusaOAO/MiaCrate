using MiaCrate.Resources;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;
using MiaCrate.Extensions;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

public delegate T RegistryBootstrap<T>(IRegistry<T> registry) where T : class;

public static class Registry
{
    public static T Register<T>(IRegistry<T> registry, ResourceLocation location, T obj) where T : class => 
        Register(registry, ResourceKey.Create<T>(registry.Key, location), obj);

    public static T Register<T>(IRegistry<T> registry, IResourceKey<T> key, T obj) where T : class
    {
        ((IWritableRegistry<T>) registry).Register(key, obj);
        return obj;
    }
}

public abstract class Registry<T> : IRegistry<T> where T : class
{
    public  IResourceKey<IRegistry> Key { get; }
    
    protected Registry(IResourceKey<IRegistry> key)
    {
        Key = key;
    }

    public abstract int GetId(T obj);
    public abstract T? ById(int id);
    public abstract T? Get(IResourceKey<T> key);
    public abstract T? Get(ResourceLocation location);
    public abstract ResourceLocation? GetKey(T obj);
    public abstract IReferenceHolder<T> CreateIntrusiveHolder(T obj);
    public abstract ISet<ResourceLocation> KeySet { get; }
}