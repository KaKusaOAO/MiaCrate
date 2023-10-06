using MiaCrate.Core;
using MiaCrate.Resources;

namespace MiaCrate.World.Damages;

public class DamageSources
{
    public IRegistry<DamageType> DamageTypes { get; }
    public DamageSource GenericKill { get; }

    public DamageSources(IRegistryAccess registryAccess)
    {
        DamageTypes = registryAccess.RegistryOrThrow(Registries.DamageType);
        GenericKill = CreateSource(Damages.DamageTypes.GenericKill);
    }

    public DamageSource CreateSource(IResourceKey<DamageType> key)
    {
        return new DamageSource(DamageTypes.GetHolderOrThrow(key));
    }
}