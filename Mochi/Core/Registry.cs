using Mochi.Extensions;
using Mochi.Resources;
using Mochi.World.Entities;
using Mochi.World.Entities.AI;
using Attribute = Mochi.World.Entities.AI.Attribute;

namespace Mochi.Core;

public delegate T RegistryBootstrap<T>(IRegistry<T> registry) where T : class;

public static class Registry
{
    private static readonly Dictionary<ResourceLocation, Func<object>> _loaders = new();

    public static ResourceLocation RootRegistryName { get; } = new("root");

    private static readonly WritableRegistry<IRegistry> _writableRegistry =
        new MappedRegistry<IRegistry>(CreateRegistryKey<IRegistry>("root"), null);

    private static IResourceKey<IRegistry<T>> CreateRegistryKey<T>(string str) where T : class => 
        ResourceKey<IRegistry<T>>.CreateRegistryKey(new ResourceLocation(str));

    public static IRegistry<IRegistry> Root => _writableRegistry;

    public static readonly IResourceKey<IRegistry<IEntityType>> EntityTypeRegistry =
        CreateRegistryKey<IEntityType>("entity_type");
    
    public static readonly IResourceKey<IRegistry<Attribute>> AttributeRegistry =
        CreateRegistryKey<Attribute>("attribute");

    public static readonly IRegistry<IEntityType> EntityType = RegisterDefaulted<IEntityType>(EntityTypeRegistry,
        "pig", e => e.BuiltinRegistryHolder, r => World.Entities.EntityType.Pig);
    
    public static readonly IRegistry<Attribute> Attribute = 
        RegisterSimple<Attribute>(AttributeRegistry, r => Attributes.Luck);
    
    private static Registry<T> RegisterSimple<T>(IResourceKey<IRegistry> key, RegistryBootstrap<T> bootstrap) 
        where T : class =>
        InternalRegister(key, new MappedRegistry<T>(key, null), bootstrap);

    private static DefaultedRegistry<T> RegisterDefaulted<T>(IResourceKey<IRegistry> key, string str,
        Func<T, IReferenceHolder<T>> provider, RegistryBootstrap<T> bootstrap) where T : class =>
        InternalRegister(key, new DefaultedRegistry<T>(str, key, provider), bootstrap);

    private static TRegistry InternalRegister<T, TRegistry>(IResourceKey<IRegistry> key, TRegistry registry,
        RegistryBootstrap<T> bootstrap) 
        where TRegistry : IWritableRegistry<T> where T : class
    {
        var location = key.Location;
        _loaders.AddOrSet(location, () => bootstrap(registry));
        _writableRegistry.Register(key, registry);
        return registry;
    }
    
    public static T Register<T>(IRegistry<T> registry, ResourceLocation location, T obj) where T : class => 
        Register(registry, ResourceKey.Create<T>(registry.Key, location), obj);

    public static T Register<T>(IRegistry<T> registry, IResourceKey<T> key, T obj) where T : class
    {
        ((IWritableRegistry<T>) registry).Register(key, obj);
        return obj;
    }

    static Registry()
    {
        // TODO: Load all the registries and register them into the root registry
        // TODO: and then check the root registry (e.g. to see if some of registries are empty)
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