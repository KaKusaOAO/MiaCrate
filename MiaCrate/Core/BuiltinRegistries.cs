using MiaCrate.Extensions;
using MiaCrate.Resources;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

public static class BuiltinRegistries
{
    private static readonly Dictionary<ResourceLocation, Func<object>> _loaders = new();
    public static ResourceLocation RootRegistryName { get; } = new("root");
    private static readonly WritableRegistry<IRegistry> _writableRegistry =
        new MappedRegistry<IRegistry>(ResourceKey<IRegistry>.CreateRegistryKey(RootRegistryName));
    public static IRegistry<IRegistry> Root => _writableRegistry;

    public static readonly IRegistry<IEntityType> EntityType = RegisterDefaultedWithIntrusiveHolders<IEntityType>(
        Registries.EntityType, "pig", _ => World.Entities.EntityType.Pig);
    public static readonly IRegistry<Attribute> Attribute = RegisterSimple<Attribute>(
        Registries.Attribute, _ => Attributes.Luck);

    private static Registry<T> RegisterSimple<T>(IResourceKey<IRegistry> key, RegistryBootstrap<T> bootstrap) 
        where T : class =>
        InternalRegister(key, new MappedRegistry<T>(key), bootstrap);

    private static DefaultedRegistry<T> RegisterDefaulted<T>(IResourceKey<IRegistry> key, string str, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedRegistry<T>(str, key, false), bootstrap);
    private static DefaultedRegistry<T> RegisterDefaultedWithIntrusiveHolders<T>(IResourceKey<IRegistry> key, string str, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedRegistry<T>(str, key, true), bootstrap);

    private static TRegistry InternalRegister<T, TRegistry>(IResourceKey<IRegistry> key, TRegistry registry,
        RegistryBootstrap<T> bootstrap) 
        where TRegistry : IWritableRegistry<T> where T : class
    {
        var location = key.Location;
        _loaders.AddOrSet(location, () => bootstrap(registry));
        _writableRegistry.Register(key, registry);
        return registry;
    }

    static BuiltinRegistries()
    {
        // TODO: Load all the registries and register them into the root registry
        // TODO: and then check the root registry (e.g. to see if some of registries are empty)
    }    
}