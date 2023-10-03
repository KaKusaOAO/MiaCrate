using MiaCrate.Resources;
using MiaCrate.Sounds;
using MiaCrate.World;
using MiaCrate.World.Blocks;
using MiaCrate.World.Entities;
using MiaCrate.World.Items;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.Core;

/// <summary>
/// This class holds all the registry key of the built-in registries,
/// which are all held by <see cref="BuiltinRegistries"/>.
/// </summary>
public static class Registries
{
    public static IResourceKey<IRegistry<Block>> Block { get; } = 
        CreateRegistryKey<Block>("block");
    
    public static IResourceKey<IRegistry<Item>> Item { get; } = 
        CreateRegistryKey<Item>("item");
    
    public static IResourceKey<IRegistry<IEntityType>> EntityType { get; } = 
        CreateRegistryKey<IEntityType>("entity_type");
    
    public static IResourceKey<IRegistry<IBlockEntityType>> BlockEntityType { get; } = 
        CreateRegistryKey<IBlockEntityType>("block_entity_type");
    
    public static IResourceKey<IRegistry<Attribute>> Attribute { get; } = 
        CreateRegistryKey<Attribute>("attribute");

    public static IResourceKey<IRegistry<IIntProviderType>> IntProviderType { get; } =
        CreateRegistryKey<IIntProviderType>("int_provider_type");
        
    public static IResourceKey<IRegistry<IFloatProviderType>> FloatProviderType { get; } =
        CreateRegistryKey<IFloatProviderType>("float_provider_type");

    public static IResourceKey<IRegistry<SoundEvent>> SoundEvent { get; } =
        CreateRegistryKey<SoundEvent>("sound_event");

    private static IResourceKey<IRegistry<T>> CreateRegistryKey<T>(string str) where T : class => 
        ResourceKey.CreateRegistryKey<IRegistry<T>>(new ResourceLocation(str));
}