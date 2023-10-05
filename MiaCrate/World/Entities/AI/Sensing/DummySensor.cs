using MiaCrate.Server.Levels;
using MiaCrate.World.Entities.AI.Memory;

namespace MiaCrate.World.Entities.AI.Sensing;

public class DummySensor : Sensor<LivingEntity>
{
    public override HashSet<IMemoryModuleType> RequiredModules { get; } = new();

    protected override void DoTick(ServerLevel level, LivingEntity entity) {}
}