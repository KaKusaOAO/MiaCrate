using MiaCrate.Resources;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;
using MiaCrate.Extensions;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

public delegate T RegistryBootstrap<T>(IRegistry<T> registry) where T : class;

/// <summary>
/// The utility class for manipulating the registries.
/// </summary>
public static class Registry
{
    public static T Register<T>(IRegistry<T> registry, string name, T obj) where T : class => 
        Register(registry, new ResourceLocation(name), obj);
    
    public static T Register<T>(IRegistry<T> registry, ResourceLocation location, T obj) where T : class => 
        Register(registry, ResourceKey.Create<T>(registry.Key, location), obj);

    public static T Register<T>(IRegistry<T> registry, IResourceKey<T> key, T obj) where T : class
    {
        ((IWritableRegistry<T>) registry).Register(key, obj);
        return obj;
    }
}