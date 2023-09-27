using System.Runtime.CompilerServices;
using MiaCrate.Data;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using MiaCrate.Sounds;
using MiaCrate.World;
using MiaCrate.World.Blocks;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;
using MiaCrate.World.Items;
using Mochi.Utils;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

/// <summary>
/// This class holds all the built-in registries in the game.
/// </summary>
public static class BuiltinRegistries
{
    private static readonly ResourceLocation _rootRegistryName = new("root");
    private static readonly Dictionary<ResourceLocation, Func<object?>> _loaders = new();
 
    /// <summary>
    /// The root registry of builtin registries.
    /// </summary>
    private static readonly IWritableRegistry<IRegistry> _writableRegistry =
        new MappedRegistry<IRegistry>(ResourceKey<IRegistry>.CreateRegistryKey(_rootRegistryName));

    #region ## Built-in registries

    public static IRegistry<SoundEvent> SoundEvent { get; } =
        RegisterSimple<SoundEvent>(Registries.SoundEvent,
            _ => SoundEvents.ItemPickup);
        

    /// <summary>
    /// The registry of all the block types.
    /// </summary>
    public static readonly IRegistry<Block> Block = 
        RegisterDefaultedWithIntrusiveHolders<Block>(Registries.Block, 
            "air", _ => World.Blocks.Block.Air);
    
    public static readonly IRegistry<Item> Item =
        RegisterDefaultedWithIntrusiveHolders<Item>(Registries.Item,
            "air", _ => World.Items.Item.Air);

    /// <summary>
    /// The registry of all the entity types.
    /// </summary>
    public static readonly IRegistry<IEntityType> EntityType = 
        RegisterDefaultedWithIntrusiveHolders<IEntityType>(Registries.EntityType, 
            "pig", _ => World.Entities.EntityType.Pig);
    
    /// <summary>
    /// The registry of all the living entity attribute types.
    /// </summary>
    public static readonly IRegistry<Attribute> Attribute = 
        RegisterSimple<Attribute>(Registries.Attribute, _ => Attributes.Luck);

    public static IRegistry<IIntProviderType> IntProviderType { get; } =
        RegisterSimple<IIntProviderType>(Registries.IntProviderType, _ => IIntProviderType.Constant);
        
    public static IRegistry<IFloatProviderType> FloatProviderType { get; } =
        RegisterSimple<IFloatProviderType>(Registries.FloatProviderType, _ => IFloatProviderType.Constant);

    #endregion
    
    public static ResourceLocation RootRegistryName => _rootRegistryName;
    public static IRegistry<IRegistry> Root => _writableRegistry;

    private static IRegistry<T> RegisterSimple<T>(IResourceKey<IRegistry> key, RegistryBootstrap<T> bootstrap) 
        where T : class =>
        InternalRegister(key, new MappedRegistry<T>(key), bootstrap, Lifecycle.Stable);
    
    private static IRegistry<T> RegisterSimple<T>(IResourceKey<IRegistry> key, Lifecycle lifecycle, 
        RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new MappedRegistry<T>(key), bootstrap, lifecycle);

    private static DefaultedMappedRegistry<T> RegisterDefaulted<T>(IResourceKey<IRegistry> key, string str,
        Lifecycle lifecycle, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedMappedRegistry<T>(str, key, false), bootstrap, lifecycle);

    private static DefaultedMappedRegistry<T> RegisterDefaultedWithIntrusiveHolders<T>(IResourceKey<IRegistry> key,
        string str, RegistryBootstrap<T> bootstrap) where T : class =>
        RegisterDefaultedWithIntrusiveHolders(key, str, Lifecycle.Stable, bootstrap);
    
    private static DefaultedMappedRegistry<T> RegisterDefaultedWithIntrusiveHolders<T>(IResourceKey<IRegistry> key, 
        string str, Lifecycle lifecycle, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedMappedRegistry<T>(str, key, true), bootstrap, lifecycle);

    private static TRegistry InternalRegister<T, TRegistry>(IResourceKey<IRegistry> key, TRegistry registry,
        RegistryBootstrap<T> bootstrap, Lifecycle lifecycle) 
        where TRegistry : IWritableRegistry<T> where T : class
    {
        var location = key.Location;
        _loaders[location] = () => bootstrap(registry);
        _writableRegistry.Register(key, registry, lifecycle);
        return registry;
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