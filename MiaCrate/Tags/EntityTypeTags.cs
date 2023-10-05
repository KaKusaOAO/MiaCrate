using MiaCrate.Core;
using MiaCrate.World.Entities;

namespace MiaCrate.Tags;

public static class EntityTypeTags
{
    public static ITagKey<IEntityType> FallDamageImmune { get; } = Create("fall_damage_immune");

    private static ITagKey<IEntityType> Create(string str) =>
        ITagKey.Create(Registries.EntityType, new ResourceLocation(str));
}