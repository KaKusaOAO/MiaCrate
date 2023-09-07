using MiaCrate.Extensions;
using MiaCrate.Resources;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;
using Mochi.Utils;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

public static class BuiltinRegistries
{
    private static readonly Dictionary<ResourceLocation, Func<object>> _loaders = new();
    public static ResourceLocation RootRegistryName { get; } = new("root");
    private static readonly IWritableRegistry<IRegistry> _writableRegistry =
        new MappedRegistry<IRegistry>(ResourceKey<IRegistry>.CreateRegistryKey(RootRegistryName));
    public static IRegistry<IRegistry> Root => _writableRegistry;

    public static readonly IRegistry<IEntityType> EntityType = RegisterDefaultedWithIntrusiveHolders<IEntityType>(
        Registries.EntityType, "pig", _ => World.Entities.EntityType.Pig);
    public static readonly IRegistry<Attribute> Attribute = RegisterSimple<Attribute>(
        Registries.Attribute, _ => Attributes.Luck);

    private static IRegistry<T> RegisterSimple<T>(IResourceKey<IRegistry> key, RegistryBootstrap<T> bootstrap) 
        where T : class =>
        InternalRegister(key, new MappedRegistry<T>(key), bootstrap);

    private static DefaultedMappedRegistry<T> RegisterDefaulted<T>(IResourceKey<IRegistry> key, string str, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedMappedRegistry<T>(str, key, false), bootstrap);
    private static DefaultedMappedRegistry<T> RegisterDefaultedWithIntrusiveHolders<T>(IResourceKey<IRegistry> key, string str, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedMappedRegistry<T>(str, key, true), bootstrap);

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

    public static void Bootstrap()
    {
        CreateContents();
        Freeze();
        Validate(_writableRegistry);
    }

    private static void CreateContents()
    {
        foreach (var (location, supplier) in _loaders)
        {
            if (supplier() == null)
            {
                Logger.Error($"Unable to bootstrap registry '{location}'");
            }
        }
    }

    private static void Freeze()
    {
        _writableRegistry.Freeze();
        foreach (var r in _writableRegistry)
        {
            r.Freeze();
        }
    }

    private static void Validate(IRegistry<IRegistry> registry)
    {
        foreach (var r in registry)
        {
            if (!r.KeySet.Any())
            {
                var location = registry.GetKey(r);
                Logger.Error($"Registry '{location}' was empty after loading");
            }

            if (r is IDefaultedRegistry defaulted)
            {
                var location = defaulted.DefaultKey;
                if (r.Get(location) == null)
                {
                    Logger.Error($"Missing default of DefaultedMappedRegistry: {location}");
                }
            }
        }
    }
}