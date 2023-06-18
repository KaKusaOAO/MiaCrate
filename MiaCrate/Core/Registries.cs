using MiaCrate.Resources;
using MiaCrate.World.Entities;

namespace MiaCrate.Core;

public static class Registries
{
    public static readonly IResourceKey<IRegistry<IEntityType>> EntityType =
        CreateRegistryKey<IEntityType>("entity_type");
    public static readonly IResourceKey<IRegistry<World.Entities.AI.Attribute>> Attribute =
        CreateRegistryKey<World.Entities.AI.Attribute>("attribute");
    
    private static IResourceKey<IRegistry<T>> CreateRegistryKey<T>(string str) where T : class => 
        ResourceKey<IRegistry<T>>.CreateRegistryKey(new ResourceLocation(str));
}