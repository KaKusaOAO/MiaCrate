using MiaCrate.Core;
using MiaCrate.Resources;

namespace MiaCrate.World.Damages;

public static class DamageTypes
{
    public static IResourceKey<DamageType> GenericKill { get; } = ResourceKey.Create<DamageType>(Registries.DamageType, new ResourceLocation("generic_kill"));
}