using MiaCrate.Core;
using MiaCrate.World.Damages;

namespace MiaCrate.Tags;

public static class DamageTypeTags
{
    public static ITagKey<DamageType> BypassesInvulnerability { get; } = Create("bypasses_invulnerability");
    public static ITagKey<DamageType> IsFall { get; } = Create("is_fall");
    public static ITagKey<DamageType> IsFire { get; } = Create("is_fire");

    private static ITagKey<DamageType> Create(string str) =>
        ITagKey.Create(Registries.DamageType, new ResourceLocation(str));
}