using MiaCrate.Resources;
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
    public static readonly IResourceKey<IRegistry<Block>> Block = 
        CreateRegistryKey<Block>("block");
    
    public static readonly IResourceKey<IRegistry<Item>> Item = 
        CreateRegistryKey<Item>("item");
    
    public static readonly IResourceKey<IRegistry<IEntityType>> EntityType = 
        CreateRegistryKey<IEntityType>("entity_type");
    
    public static readonly IResourceKey<IRegistry<Attribute>> Attribute = 
        CreateRegistryKey<Attribute>("attribute");
    
    private static IResourceKey<IRegistry<T>> CreateRegistryKey<T>(string str) where T : class => 
        ResourceKey<IRegistry<T>>.CreateRegistryKey(new ResourceLocation(str));
}