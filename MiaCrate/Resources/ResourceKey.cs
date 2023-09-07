using System.Diagnostics;
using MiaCrate.Core;
using MiaCrate.Extensions;

namespace MiaCrate.Resources;

public static class ResourceKey
{
    public static IResourceKey<T> CreateRegistryKey<T>(ResourceLocation location) where T : class => 
        ResourceKey<T>.CreateRegistryKey(location);
    public static IResourceKey<T> Create<T, TRegistry>(IResourceKey<TRegistry> key, ResourceLocation location) 
        where TRegistry : IRegistry<T> where T : class => 
        ResourceKey<T>.Create(key, location);
    public static IResourceKey<T> Create<T>(IResourceKey<IRegistry> key, ResourceLocation location) where T : class => 
        ResourceKey<T>.Create(key, location);
}

public class ResourceKey<T> : IResourceKey<T> where T : class
{
    private static Dictionary<string, IResourceKey> _values = new();
    
    public ResourceLocation Registry { get; }
    public ResourceLocation Location { get; }

    private ResourceKey(ResourceLocation registry, ResourceLocation location)
    {
        Registry = registry;
        Location = location;
    }
    
    public static IResourceKey<T> CreateRegistryKey(ResourceLocation location) => 
        Create(BuiltinRegistries.RootRegistryName, location);

    public static IResourceKey<T> Create<TRegistry>(IResourceKey<TRegistry> key, ResourceLocation location) 
        where TRegistry : IRegistry<T> => 
        Create(key.Location, location);
    
    public static IResourceKey<T> Create(IResourceKey<IRegistry> key, ResourceLocation location) => 
        Create(key.Location, location);

    private static IResourceKey<T> Create(ResourceLocation registry, ResourceLocation location)
    {
        var str = $"{registry}:{location}";
        return (IResourceKey<T>) _values.ComputeIfAbsent(str, _ => new ResourceKey<T>(registry, location));
    }

    public override string ToString() => $"ResourceKey[{Registry} / {Location}]";
}